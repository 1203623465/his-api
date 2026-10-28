using His.Api.Dtos;
using His.Api.Middleware;
using His.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace His.Api.Services;

/// <summary>
/// 收费业务服务：处方划价收费、退费、发药。
/// <para>核心业务规则：</para>
/// <list type="bullet">
/// <item>收费：仅"未收费(1)"状态处方可收费；事务内按效期先进先出(FEFO)扣减库存并生成收费单</item>
/// <item>退费：仅"已收费(2)"状态处方可退费；事务内回补库存、写入退款记录、处方置为"已退款(4)"</item>
/// <item>发药：仅"已收费(2)"状态处方可发药；写发药记录，处方置为"已发药(3)"</item>
/// </list>
/// 状态流转：1 未收费 → 2 已收费 → 3 已发药 / 4 已退款。
/// </summary>
public interface IBillingService
{
    Task<PageResult<PayRecordDto>> GetPagedAsync(PageRequest req);
    Task<PayRecordDto> ChargeAsync(ChargeRequest req);
    Task RefundAsync(RefundRequest req);
    Task DispenseAsync(DispenseRequest req);
}

public class BillingService : IBillingService
{
    private readonly HisDbContext _db;
    private readonly ILogger<BillingService> _logger;

    public BillingService(HisDbContext db, ILogger<BillingService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>收费记录分页查询（含患者姓名与是否已退费标记）。</summary>
    public async Task<PageResult<PayRecordDto>> GetPagedAsync(PageRequest req)
    {
        var query = _db.PayRecords.AsQueryable();
        if (!string.IsNullOrWhiteSpace(req.Keyword))
        {
            // 关键字按患者姓名匹配：先解析患者ID集合，再过滤收费记录
            var patientIds = await _db.Patients
                .Where(p => p.PatientName != null && p.PatientName.Contains(req.Keyword))
                .Select(p => p.Id)
                .ToListAsync();
            query = query.Where(p => p.PatientId.HasValue && patientIds.Contains(p.PatientId.Value));
        }

        var total = await query.LongCountAsync();
        var entities = await query.OrderByDescending(p => p.Id).Skip(req.Skip).Take(req.Size).ToListAsync();

        var patientIds2 = entities.Where(e => e.PatientId.HasValue).Select(e => e.PatientId!.Value).Distinct().ToList();
        var patients = await _db.Patients.Where(p => patientIds2.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.PatientName);
        var payIds = entities.Select(e => e.Id).ToList();
        // 已产生退款记录的收费单视为"已退费"
        var refundedIds = (await _db.RefundRecords.Where(r => r.OriginalPayId.HasValue && payIds.Contains(r.OriginalPayId.Value))
            .Select(r => r.OriginalPayId!.Value).Distinct().ToListAsync()).ToHashSet();

        var items = entities.Select(e => new PayRecordDto(
            e.Id, e.PatientId,
            e.PatientId.HasValue && patients.TryGetValue(e.PatientId.Value, out var pn) ? pn : null,
            e.RegId, e.TotalAmount, e.PayType, e.InvoiceNo, e.PayTime,
            refundedIds.Contains(e.Id))).ToList();

        return new PageResult<PayRecordDto>(total, items);
    }

    /// <summary>
    /// 划价收费：校验处方状态 → 校验库存（按药品汇总）→ 事务内扣库存 + 生成收费单。
    /// </summary>
    public async Task<PayRecordDto> ChargeAsync(ChargeRequest req)
    {
        var pres = await _db.Prescriptions.FindAsync(req.PresId)
            ?? throw new ApiException("处方不存在", 404);
        if (pres.PresStatus != 1)
        {
            throw new ApiException("仅未收费状态的处方可以收费");
        }

        var details = await _db.PrescriptionDetails.Where(d => d.PresId == pres.Id).ToListAsync();
        if (details.Count == 0)
        {
            throw new ApiException("处方无药品明细，不能收费");
        }

        var visit = pres.VisitId.HasValue ? await _db.ClinicVisits.FindAsync(pres.VisitId.Value) : null;

        await using var tx = await _db.Database.BeginTransactionAsync();

        // 库存校验：同一药品可能在处方中出现多行，必须先按药品汇总总需求量再校验，
        // 否则"两行各要60、库存100"会逐行误判通过，实际总需求120已超库存。
        var demandByDrug = details
            .Where(d => d.DrugId.HasValue)
            .GroupBy(d => d.DrugId!.Value)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity ?? 0m));

        var drugNames = await _db.Drugs
            .Where(d => demandByDrug.Keys.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, d => d.DrugName);

        foreach (var (drugId, demand) in demandByDrug)
        {
            var available = await _db.DrugStocks
                .Where(s => s.DrugId == drugId)
                .SumAsync(s => s.Quantity ?? 0m);
            if (available < demand)
            {
                var name = drugNames.TryGetValue(drugId, out var n) ? n : drugId.ToString();
                throw new ApiException($"药品「{name}」库存不足（需 {demand}，现有 {available}），无法收费");
            }
        }

        // 逐行扣减库存：按效期先进先出(FEFO)，跨批次拼接扣减
        foreach (var detail in details)
        {
            var remaining = detail.Quantity ?? 0m;
            var batches = await _db.DrugStocks
                .Where(s => s.DrugId == detail.DrugId && s.Quantity > 0)
                .OrderBy(s => s.ExpireDate)
                .ToListAsync();

            foreach (var batch in batches)
            {
                if (remaining <= 0)
                {
                    break;
                }
                var take = Math.Min(batch.Quantity ?? 0m, remaining);
                batch.Quantity -= take;
                remaining -= take;
            }
        }

        var totalAmount = details.Sum(d => d.Amount ?? 0m);
        var pay = new PayRecord
        {
            PatientId = pres.PatientId,
            RegId = visit?.RegId,
            TotalAmount = totalAmount,
            PayType = req.PayType,
            InvoiceNo = $"INV{DateTime.Now:yyyyMMddHHmmssfff}",
            PayTime = DateTime.Now
        };
        _db.PayRecords.Add(pay);
        pres.PresStatus = 2;
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        _logger.LogInformation("收费成功: 处方 {PresId}, 金额 {Amount}, 发票 {InvoiceNo}", pres.Id, totalAmount, pay.InvoiceNo);

        var patientName = await _db.Patients.Where(p => p.Id == pres.PatientId).Select(p => p.PatientName).FirstOrDefaultAsync();
        return new PayRecordDto(pay.Id, pay.PatientId, patientName, pay.RegId, pay.TotalAmount, pay.PayType, pay.InvoiceNo, pay.PayTime, false);
    }

    /// <summary>
    /// 退费：将处方对应的收费单退款，回补药品库存。
    /// <para>收费单匹配规则：同一挂号可能对应多张处方/多张收费单（pay_record 无处方外键），
    /// 因此按「关联挂号 + 金额一致 + 尚未退费」三个条件定位当前处方对应的收费单，
    /// 避免退错其他处方的收费单。</para>
    /// </summary>
    public async Task RefundAsync(RefundRequest req)
    {
        var pres = await _db.Prescriptions.FindAsync(req.PresId)
            ?? throw new ApiException("处方不存在", 404);
        if (pres.PresStatus != 2)
        {
            throw new ApiException("仅已收费状态的处方可以退费");
        }

        var details = await _db.PrescriptionDetails.Where(d => d.PresId == pres.Id).ToListAsync();
        var presAmount = details.Sum(d => d.Amount ?? 0m);

        // 定位候选收费单：同挂号的收费单中金额与当前处方一致的
        var visit = pres.VisitId.HasValue ? await _db.ClinicVisits.FindAsync(pres.VisitId.Value) : null;
        var regId = visit?.RegId;
        var candidates = await _db.PayRecords
            .Where(p => p.RegId == regId && p.TotalAmount == presAmount)
            .OrderByDescending(p => p.Id)
            .ToListAsync();
        if (candidates.Count == 0)
        {
            throw new ApiException("未找到对应的收费记录");
        }

        // 排除已退费的收费单，剩余的第一张即当前处方对应的收费单
        var refundedIds = (await _db.RefundRecords
            .Where(r => r.OriginalPayId.HasValue)
            .Select(r => r.OriginalPayId!.Value)
            .ToListAsync()).ToHashSet();
        var pay = candidates.FirstOrDefault(c => !refundedIds.Contains(c.Id))
            ?? throw new ApiException("该处方对应的收费单已退费");

        await using var tx = await _db.Database.BeginTransactionAsync();

        // 回补库存：优先回到最早效期批次，药品已无批次则新建一条
        foreach (var detail in details)
        {
            var batch = await _db.DrugStocks
                .Where(s => s.DrugId == detail.DrugId)
                .OrderBy(s => s.ExpireDate)
                .FirstOrDefaultAsync();
            if (batch != null)
            {
                batch.Quantity = (batch.Quantity ?? 0m) + (detail.Quantity ?? 0m);
            }
            else
            {
                _db.DrugStocks.Add(new DrugStock
                {
                    DrugId = detail.DrugId,
                    Quantity = detail.Quantity,
                    BatchNo = $"RF{pres.Id}",
                    ExpireDate = DateTime.Now.AddYears(1)
                });
            }
        }

        _db.RefundRecords.Add(new RefundRecord
        {
            OriginalPayId = pay.Id,
            RefundAmount = pay.TotalAmount,
            RefundTime = DateTime.Now,
            Reason = req.Reason
        });
        pres.PresStatus = 4;
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        _logger.LogInformation("退费成功: 处方 {PresId}, 原收费单 {PayId}", pres.Id, pay.Id);
    }

    /// <summary>发药：写发药记录，处方置为已发药。库存已在收费时扣减，此处不再变动。</summary>
    public async Task DispenseAsync(DispenseRequest req)
    {
        var pres = await _db.Prescriptions.FindAsync(req.PresId)
            ?? throw new ApiException("处方不存在", 404);
        if (pres.PresStatus != 2)
        {
            throw new ApiException("仅已收费状态的处方可以发药");
        }

        _db.DispenseRecords.Add(new DispenseRecord
        {
            PresId = pres.Id,
            PatientId = pres.PatientId,
            DispenseTime = DateTime.Now,
            Operator = req.Operator
        });
        pres.PresStatus = 3;
        await _db.SaveChangesAsync();

        _logger.LogInformation("发药完成: 处方 {PresId}", pres.Id);
    }
}

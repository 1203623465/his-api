using His.Api.Dtos;
using His.Api.Middleware;
using His.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace His.Api.Services;

/// <summary>
/// 处方业务服务。
/// <para>核心业务规则：</para>
/// <list type="bullet">
/// <item>开方校验：就诊记录须存在；药品须存在且"已启用"（drug.is_active = 1）</item>
/// <item>明细计价：单价取开方时刻的药品零售价，小计 = 数量 × 单价（四舍五入到分）</item>
/// <item>初始状态：1 未收费；后续流转见 BillingService（2 已收费 / 3 已发药 / 4 已退款）</item>
/// <item>一个就诊记录允许多次开方（复诊补充处方场景）</item>
/// </list>
/// 处方头与明细在同一事务内写入，保证不会出现"有头无明细"的脏数据。
/// </summary>
public interface IPrescriptionService
{
    Task<PageResult<PrescriptionListItemDto>> GetPagedAsync(PageRequest req, sbyte? status);
    Task<PrescriptionDetailDto> GetDetailAsync(long id);
    Task<PrescriptionDetailDto> CreateAsync(PrescriptionCreateRequest req);
}

public class PrescriptionService : IPrescriptionService
{
    private readonly HisDbContext _db;

    public PrescriptionService(HisDbContext db)
    {
        _db = db;
    }

    private async Task<(Dictionary<long, string?> Patients, Dictionary<long, string?> Doctors)> GetNameMapsAsync(
        IEnumerable<long?> patientIds, IEnumerable<long?> doctorIds)
    {
        var pIds = patientIds.Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
        var dIds = doctorIds.Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
        var patients = await _db.Patients.Where(p => pIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.PatientName);
        var doctors = await _db.Doctors.Where(d => dIds.Contains(d.Id)).ToDictionaryAsync(d => d.Id, d => d.DoctorName);
        return (patients, doctors);
    }

    public async Task<PageResult<PrescriptionListItemDto>> GetPagedAsync(PageRequest req, sbyte? status)
    {
        var query = _db.Prescriptions.AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(p => p.PresStatus == status.Value);
        }
        if (!string.IsNullOrWhiteSpace(req.Keyword))
        {
            var patientIds = await _db.Patients
                .Where(p => p.PatientName != null && p.PatientName.Contains(req.Keyword))
                .Select(p => p.Id)
                .ToListAsync();
            query = query.Where(p => p.PatientId.HasValue && patientIds.Contains(p.PatientId.Value));
        }

        var total = await query.LongCountAsync();
        var entities = await query.OrderByDescending(p => p.Id).Skip(req.Skip).Take(req.Size).ToListAsync();

        var presIds = entities.Select(e => e.Id).ToList();
        var amounts = await _db.PrescriptionDetails
            .Where(d => d.PresId.HasValue && presIds.Contains(d.PresId.Value))
            .GroupBy(d => d.PresId!.Value)
            .Select(g => new { PresId = g.Key, Total = g.Sum(x => x.Amount ?? 0) })
            .ToDictionaryAsync(x => x.PresId, x => x.Total);

        var (patients, doctors) = await GetNameMapsAsync(entities.Select(e => e.PatientId), entities.Select(e => e.DoctorId));

        var items = entities.Select(e => new PrescriptionListItemDto(
            e.Id, e.VisitId,
            e.PatientId,
            e.PatientId.HasValue && patients.TryGetValue(e.PatientId.Value, out var pn) ? pn : null,
            e.DoctorId,
            e.DoctorId.HasValue && doctors.TryGetValue(e.DoctorId.Value, out var dn) ? dn : null,
            e.PresTime, e.PresStatus,
            amounts.TryGetValue(e.Id, out var amt) ? amt : 0m)).ToList();

        return new PageResult<PrescriptionListItemDto>(total, items);
    }

    public async Task<PrescriptionDetailDto> GetDetailAsync(long id)
    {
        var entity = await _db.Prescriptions.FindAsync(id)
            ?? throw new ApiException("处方不存在", 404);

        var details = await _db.PrescriptionDetails.Where(d => d.PresId == id).ToListAsync();
        var drugIds = details.Where(d => d.DrugId.HasValue).Select(d => d.DrugId!.Value).Distinct().ToList();
        var drugs = await _db.Drugs.Where(d => drugIds.Contains(d.Id)).ToDictionaryAsync(d => d.Id);
        var (patients, doctors) = await GetNameMapsAsync(new[] { entity.PatientId }, new[] { entity.DoctorId });

        var items = details.Select(d => new PrescriptionItemDto(
            d.Id, d.DrugId,
            d.DrugId.HasValue && drugs.TryGetValue(d.DrugId.Value, out var drug) ? drug.DrugName : null,
            d.DrugId.HasValue && drugs.TryGetValue(d.DrugId.Value, out var drug2) ? drug2.DrugSpec : null,
            d.DrugId.HasValue && drugs.TryGetValue(d.DrugId.Value, out var drug3) ? drug3.DrugUnit : null,
            d.Quantity, d.UnitPrice, d.Amount)).ToList();

        return new PrescriptionDetailDto(
            entity.Id, entity.VisitId,
            entity.PatientId,
            entity.PatientId.HasValue && patients.TryGetValue(entity.PatientId.Value, out var pn) ? pn : null,
            entity.DoctorId,
            entity.DoctorId.HasValue && doctors.TryGetValue(entity.DoctorId.Value, out var dn) ? dn : null,
            entity.PresTime, entity.PresStatus,
            details.Sum(d => d.Amount ?? 0m), items);
    }

    public async Task<PrescriptionDetailDto> CreateAsync(PrescriptionCreateRequest req)
    {
        var visit = await _db.ClinicVisits.FindAsync(req.VisitId)
            ?? throw new ApiException("就诊记录不存在", 404);

        var drugIds = req.Items.Select(i => i.DrugId).Distinct().ToList();
        var drugs = await _db.Drugs.Where(d => drugIds.Contains(d.Id)).ToDictionaryAsync(d => d.Id);
        foreach (var item in req.Items)
        {
            if (!drugs.TryGetValue(item.DrugId, out var drug))
            {
                throw new ApiException($"药品(ID:{item.DrugId})不存在");
            }
            if (drug.IsActive != 1)
            {
                throw new ApiException($"药品「{drug.DrugName}」未启用，不能开具");
            }
        }

        await using var tx = await _db.Database.BeginTransactionAsync();

        var pres = new Prescription
        {
            VisitId = visit.Id,
            PatientId = visit.PatientId,
            DoctorId = visit.DoctorId,
            PresTime = DateTime.Now,
            PresStatus = 1
        };
        _db.Prescriptions.Add(pres);
        await _db.SaveChangesAsync();

        foreach (var item in req.Items)
        {
            var drug = drugs[item.DrugId];
            var unitPrice = drug.DrugPrice ?? 0m;
            _db.PrescriptionDetails.Add(new PrescriptionDetail
            {
                PresId = pres.Id,
                DrugId = item.DrugId,
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
                Amount = Math.Round(item.Quantity * unitPrice, 2)
            });
        }
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return await GetDetailAsync(pres.Id);
    }
}

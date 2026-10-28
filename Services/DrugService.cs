using His.Api.Dtos;
using His.Api.Middleware;
using His.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace His.Api.Services;

/// <summary>
/// 药品与库存业务服务。
/// <para>业务规则：药品名称唯一；删除前校验是否被处方明细引用（有引用则拒绝删除，
/// 同时级联删除其库存批次）。启用状态：1 已启用（可开方），0 未启用。</para>
/// <para>库存模型：一个药品多个批次（批号/效期），药品总库存 = 各批次数量之和；
/// 收费扣减的先进先出策略见 BillingService。</para>
/// </summary>
public interface IDrugService
{
    Task<PageResult<DrugListItemDto>> GetPagedAsync(PageRequest req);
    Task<IReadOnlyList<DrugListItemDto>> GetActiveListAsync();
    Task<DrugListItemDto> CreateAsync(DrugSaveRequest req);
    Task<DrugListItemDto> UpdateAsync(long id, DrugSaveRequest req);
    Task DeleteAsync(long id);
    Task<IReadOnlyList<DrugStockDto>> GetStocksAsync(long drugId);
    Task<DrugStockDto> AddStockAsync(long drugId, StockAddRequest req);
    Task<DrugStockDto> UpdateStockAsync(long stockId, StockUpdateRequest req);
}

public class DrugService : IDrugService
{
    private readonly HisDbContext _db;

    public DrugService(HisDbContext db)
    {
        _db = db;
    }

    private async Task<Dictionary<long, decimal>> GetStockTotalsAsync(IEnumerable<long> drugIds)
    {
        var ids = drugIds.ToList();
        return await _db.DrugStocks
            .Where(s => s.DrugId.HasValue && ids.Contains(s.DrugId.Value))
            .GroupBy(s => s.DrugId!.Value)
            .Select(g => new { DrugId = g.Key, Total = g.Sum(x => x.Quantity ?? 0) })
            .ToDictionaryAsync(x => x.DrugId, x => x.Total);
    }

    private static DrugListItemDto ToDto(Drug d, IReadOnlyDictionary<long, decimal> stockTotals)
        => new(d.Id, d.DrugName, d.DrugSpec, d.DrugUnit, d.DrugPrice, d.IsActive,
            stockTotals.TryGetValue(d.Id, out var total) ? total : 0m);

    public async Task<PageResult<DrugListItemDto>> GetPagedAsync(PageRequest req)
    {
        var query = _db.Drugs.AsQueryable();
        if (!string.IsNullOrWhiteSpace(req.Keyword))
        {
            query = query.Where(d => d.DrugName != null && d.DrugName.Contains(req.Keyword));
        }

        var total = await query.LongCountAsync();
        var entities = await query.OrderBy(d => d.Id).Skip(req.Skip).Take(req.Size).ToListAsync();
        var stockTotals = await GetStockTotalsAsync(entities.Select(e => e.Id));
        return new PageResult<DrugListItemDto>(total, entities.Select(d => ToDto(d, stockTotals)).ToList());
    }

    public async Task<IReadOnlyList<DrugListItemDto>> GetActiveListAsync()
    {
        var entities = await _db.Drugs.Where(d => d.IsActive == 1).OrderBy(d => d.Id).ToListAsync();
        var stockTotals = await GetStockTotalsAsync(entities.Select(e => e.Id));
        return entities.Select(d => ToDto(d, stockTotals)).ToList();
    }

    public async Task<DrugListItemDto> CreateAsync(DrugSaveRequest req)
    {
        if (await _db.Drugs.AnyAsync(d => d.DrugName == req.DrugName))
        {
            throw new ApiException("药品名称已存在");
        }

        var entity = new Drug
        {
            DrugName = req.DrugName,
            DrugSpec = req.DrugSpec,
            DrugUnit = req.DrugUnit,
            DrugPrice = req.DrugPrice,
            IsActive = req.IsActive
        };
        _db.Drugs.Add(entity);
        await _db.SaveChangesAsync();
        return ToDto(entity, new Dictionary<long, decimal>());
    }

    public async Task<DrugListItemDto> UpdateAsync(long id, DrugSaveRequest req)
    {
        var entity = await _db.Drugs.FindAsync(id)
            ?? throw new ApiException("药品不存在", 404);
        if (await _db.Drugs.AnyAsync(d => d.DrugName == req.DrugName && d.Id != id))
        {
            throw new ApiException("药品名称已存在");
        }

        entity.DrugName = req.DrugName;
        entity.DrugSpec = req.DrugSpec;
        entity.DrugUnit = req.DrugUnit;
        entity.DrugPrice = req.DrugPrice;
        entity.IsActive = req.IsActive;
        await _db.SaveChangesAsync();
        var stockTotals = await GetStockTotalsAsync(new[] { id });
        return ToDto(entity, stockTotals);
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _db.Drugs.FindAsync(id)
            ?? throw new ApiException("药品不存在", 404);

        if (await _db.PrescriptionDetails.AnyAsync(d => d.DrugId == id))
        {
            throw new ApiException("该药品已被处方引用，不能删除");
        }

        var stocks = await _db.DrugStocks.Where(s => s.DrugId == id).ToListAsync();
        _db.DrugStocks.RemoveRange(stocks);
        _db.Drugs.Remove(entity);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<DrugStockDto>> GetStocksAsync(long drugId)
    {
        if (!await _db.Drugs.AnyAsync(d => d.Id == drugId))
        {
            throw new ApiException("药品不存在", 404);
        }

        return await _db.DrugStocks
            .Where(s => s.DrugId == drugId)
            .OrderBy(s => s.ExpireDate)
            .Select(s => new DrugStockDto(s.Id, s.DrugId, s.Quantity, s.BatchNo, s.ExpireDate))
            .ToListAsync();
    }

    public async Task<DrugStockDto> AddStockAsync(long drugId, StockAddRequest req)
    {
        if (!await _db.Drugs.AnyAsync(d => d.Id == drugId))
        {
            throw new ApiException("药品不存在", 404);
        }

        var entity = new DrugStock
        {
            DrugId = drugId,
            Quantity = req.Quantity,
            BatchNo = req.BatchNo,
            ExpireDate = req.ExpireDate
        };
        _db.DrugStocks.Add(entity);
        await _db.SaveChangesAsync();
        return new DrugStockDto(entity.Id, entity.DrugId, entity.Quantity, entity.BatchNo, entity.ExpireDate);
    }

    public async Task<DrugStockDto> UpdateStockAsync(long stockId, StockUpdateRequest req)
    {
        var entity = await _db.DrugStocks.FindAsync(stockId)
            ?? throw new ApiException("库存批次不存在", 404);

        entity.Quantity = req.Quantity;
        entity.BatchNo = req.BatchNo;
        entity.ExpireDate = req.ExpireDate;
        await _db.SaveChangesAsync();
        return new DrugStockDto(entity.Id, entity.DrugId, entity.Quantity, entity.BatchNo, entity.ExpireDate);
    }
}

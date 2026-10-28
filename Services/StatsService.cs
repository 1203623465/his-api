using His.Api.Dtos;
using His.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace His.Api.Services;

/// <summary>
/// 仪表盘统计服务。
/// 指标口径：今日挂号（reg_time 当日）；待收费（pres_status = 1）；
/// 今日收入（pay_record.pay_time 当日合计，不含已退费扣减）；
/// 库存预警（药品总库存 &lt; LowStockThreshold）；近 7 日挂号趋势（缺数日期补 0）。
/// </summary>
public interface IStatsService
{
    Task<DashboardDto> GetDashboardAsync();
}

public class StatsService : IStatsService
{
    /// <summary>库存预警阈值：药品总库存低于该值计入预警</summary>
    private const decimal LowStockThreshold = 50m;

    private readonly HisDbContext _db;

    public StatsService(HisDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var todayRegistrations = await _db.ClinicRegs
            .LongCountAsync(r => r.RegTime >= today && r.RegTime < tomorrow);

        var pendingCharges = await _db.Prescriptions.LongCountAsync(p => p.PresStatus == 1);

        var todayRevenue = await _db.PayRecords
            .Where(p => p.PayTime >= today && p.PayTime < tomorrow)
            .SumAsync(p => p.TotalAmount ?? 0m);

        var stockTotals = await _db.DrugStocks
            .Where(s => s.DrugId.HasValue)
            .GroupBy(s => s.DrugId!.Value)
            .Select(g => g.Sum(x => x.Quantity ?? 0m))
            .ToListAsync();
        var lowStockDrugs = (long)stockTotals.Count(t => t < LowStockThreshold);

        var trendStart = today.AddDays(-6);
        var trendRaw = await _db.ClinicRegs
            .Where(r => r.RegTime >= trendStart && r.RegTime < tomorrow)
            .GroupBy(r => r.RegTime!.Value.Date)
            .Select(g => new { Date = g.Key, Count = g.LongCount() })
            .ToListAsync();

        var trend = Enumerable.Range(0, 7)
            .Select(i =>
            {
                var date = trendStart.AddDays(i);
                var count = trendRaw.FirstOrDefault(x => x.Date == date)?.Count ?? 0;
                return new DailyCountDto(date.ToString("MM-dd"), count);
            })
            .ToList();

        return new DashboardDto(todayRegistrations, pendingCharges, todayRevenue, lowStockDrugs, trend);
    }
}

namespace His.Api.Dtos;

public class DailyCountDto
{
    public string Date { get; set; }
    public long Count { get; set; }

    public DailyCountDto(string date, long count)
    {
        Date = date;
        Count = count;
    }
}

public class DashboardDto
{
    public long TodayRegistrations { get; set; }
    public long PendingCharges { get; set; }
    public decimal TodayRevenue { get; set; }
    public long LowStockDrugs { get; set; }
    public IReadOnlyList<DailyCountDto> RegTrend { get; set; }

    public DashboardDto(
        long todayRegistrations,
        long pendingCharges,
        decimal todayRevenue,
        long lowStockDrugs,
        IReadOnlyList<DailyCountDto> regTrend)
    {
        TodayRegistrations = todayRegistrations;
        PendingCharges = pendingCharges;
        TodayRevenue = todayRevenue;
        LowStockDrugs = lowStockDrugs;
        RegTrend = regTrend;
    }
}

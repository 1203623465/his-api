using His.Api.Dtos;
using His.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace His.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private readonly IStatsService _service;

    public StatsController(IStatsService service)
    {
        _service = service;
    }

    /// <summary>仪表盘统计：今日挂号/待收费/今日收入/库存预警/近7日趋势</summary>
    [HttpGet("dashboard")]
    [SwaggerOperation(
        Summary = "获取仪表盘统计",
        Description = "一次性返回今日挂号数、待收费数、今日收入、库存预警数与近 7 日挂号趋势",
        Tags = new[] { "统计分析" })]
    public async Task<ApiResult<DashboardDto>> GetDashboard()
    {
        var result = await _service.GetDashboardAsync();
        return ApiResult<DashboardDto>.Ok(result);
    }
}

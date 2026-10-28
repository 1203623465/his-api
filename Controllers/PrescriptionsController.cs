using His.Api.Dtos;
using His.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace His.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrescriptionsController : ControllerBase
{
    private readonly IPrescriptionService _service;

    public PrescriptionsController(IPrescriptionService service)
    {
        _service = service;
    }

    /// <summary>分页查询处方，可按状态过滤</summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "分页查询处方",
        Description = "支持关键字与处方状态（status）过滤，支持分页",
        Tags = new[] { "处方管理" })]
    public async Task<ApiResult<PageResult<PrescriptionListItemDto>>> GetPaged(
        [FromQuery] PageRequest req, [FromQuery] sbyte? status)
    {
        var result = await _service.GetPagedAsync(req, status);
        return ApiResult<PageResult<PrescriptionListItemDto>>.Ok(result);
    }

    /// <summary>查询处方详情（含药品明细）</summary>
    [HttpGet("{id:long}")]
    [SwaggerOperation(
        Summary = "查询处方详情",
        Description = "按处方主键返回处方头信息及其全部药品明细",
        Tags = new[] { "处方管理" })]
    public async Task<ApiResult<PrescriptionDetailDto>> GetDetail(long id)
    {
        var result = await _service.GetDetailAsync(id);
        return ApiResult<PrescriptionDetailDto>.Ok(result);
    }

    /// <summary>开具处方（含明细，事务写入）</summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "开具处方",
        Description = "为指定就诊记录开具处方，处方头与明细在同一事务内写入，至少包含一条药品",
        Tags = new[] { "处方管理" })]
    public async Task<ApiResult<PrescriptionDetailDto>> Create(PrescriptionCreateRequest req)
    {
        var result = await _service.CreateAsync(req);
        return ApiResult<PrescriptionDetailDto>.Ok(result, "处方开具成功");
    }
}

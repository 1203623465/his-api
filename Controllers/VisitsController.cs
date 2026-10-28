using His.Api.Dtos;
using His.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace His.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VisitsController : ControllerBase
{
    private readonly IVisitService _service;

    public VisitsController(IVisitService service)
    {
        _service = service;
    }

    /// <summary>分页查询就诊记录</summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "分页查询就诊记录",
        Description = "支持关键字模糊查询与分页，返回就诊记录列表",
        Tags = new[] { "就诊管理" })]
    public async Task<ApiResult<PageResult<VisitListItemDto>>> GetPaged([FromQuery] PageRequest req)
    {
        var result = await _service.GetPagedAsync(req);
        return ApiResult<PageResult<VisitListItemDto>>.Ok(result);
    }

    /// <summary>按主键查询就诊记录</summary>
    [HttpGet("{id:long}")]
    [SwaggerOperation(
        Summary = "查询就诊记录详情",
        Description = "按就诊记录主键返回详情，含主诉与诊断信息",
        Tags = new[] { "就诊管理" })]
    public async Task<ApiResult<VisitListItemDto>> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id);
        return ApiResult<VisitListItemDto>.Ok(result);
    }

    /// <summary>开始就诊：由挂号单生成就诊记录</summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "开始就诊",
        Description = "根据挂号单生成就诊记录，并将挂号状态置为已就诊",
        Tags = new[] { "就诊管理" })]
    public async Task<ApiResult<VisitListItemDto>> Start(VisitStartRequest req)
    {
        var result = await _service.StartAsync(req);
        return ApiResult<VisitListItemDto>.Ok(result, "就诊已开始");
    }

    /// <summary>保存病历（主诉/诊断）</summary>
    [HttpPut("{id:long}")]
    [SwaggerOperation(
        Summary = "保存病历",
        Description = "按就诊记录主键保存主诉与诊断内容",
        Tags = new[] { "就诊管理" })]
    public async Task<ApiResult<VisitListItemDto>> SaveRecord(long id, VisitSaveRequest req)
    {
        var result = await _service.SaveRecordAsync(id, req);
        return ApiResult<VisitListItemDto>.Ok(result, "病历已保存");
    }
}

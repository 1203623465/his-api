using His.Api.Dtos;
using His.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace His.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrationsController : ControllerBase
{
    private readonly IRegistrationService _service;

    public RegistrationsController(IRegistrationService service)
    {
        _service = service;
    }

    /// <summary>分页查询挂号记录，可按状态/日期过滤</summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "分页查询挂号记录",
        Description = "支持关键字、挂号状态（status）、挂号日期（date）过滤，支持分页",
        Tags = new[] { "挂号管理" })]
    public async Task<ApiResult<PageResult<RegistrationListItemDto>>> GetPaged(
        [FromQuery] PageRequest req, [FromQuery] sbyte? status, [FromQuery] DateTime? date)
    {
        var result = await _service.GetPagedAsync(req, status, date);
        return ApiResult<PageResult<RegistrationListItemDto>>.Ok(result);
    }

    /// <summary>候诊队列（已挂号未就诊）</summary>
    [HttpGet("queue")]
    [SwaggerOperation(
        Summary = "获取候诊队列",
        Description = "返回已挂号但尚未开始就诊的记录，可按医生（doctorId）过滤",
        Tags = new[] { "挂号管理" })]
    public async Task<ApiResult<IReadOnlyList<RegistrationListItemDto>>> GetQueue([FromQuery] long? doctorId)
    {
        var result = await _service.GetQueueAsync(doctorId);
        return ApiResult<IReadOnlyList<RegistrationListItemDto>>.Ok(result);
    }

    /// <summary>新增挂号</summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "新增挂号",
        Description = "为患者挂号，指定科室与医生，生成待就诊的挂号记录",
        Tags = new[] { "挂号管理" })]
    public async Task<ApiResult<RegistrationListItemDto>> Create(RegistrationCreateRequest req)
    {
        var result = await _service.CreateAsync(req);
        return ApiResult<RegistrationListItemDto>.Ok(result, "挂号成功");
    }

    /// <summary>退号</summary>
    [HttpPost("{id:long}/cancel")]
    [SwaggerOperation(
        Summary = "退号",
        Description = "取消挂号记录并回写状态；已就诊的记录不允许退号",
        Tags = new[] { "挂号管理" })]
    public async Task<ApiResult<object>> Cancel(long id)
    {
        await _service.CancelAsync(id);
        return ApiResult.Ok("退号成功");
    }
}

using His.Api.Dtos;
using His.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace His.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BillingController : ControllerBase
{
    private readonly IBillingService _service;

    public BillingController(IBillingService service)
    {
        _service = service;
    }

    /// <summary>分页查询收费记录</summary>
    [HttpGet("records")]
    [SwaggerOperation(
        Summary = "获取收费记录",
        Description = "按关键字分页查询收费单，支持按患者姓名/单号模糊匹配",
        Tags = new[] { "费用管理" })]
    public async Task<ApiResult<PageResult<PayRecordDto>>> GetPaged([FromQuery] PageRequest req)
    {
        var result = await _service.GetPagedAsync(req);
        return ApiResult<PageResult<PayRecordDto>>.Ok(result);
    }

    /// <summary>划价收费：生成收费单并扣减库存（事务）</summary>
    [HttpPost("charge")]
    [SwaggerOperation(
        Summary = "划价收费",
        Description = "根据处方生成收费单并扣减药品库存，全过程在同一事务内提交",
        Tags = new[] { "费用管理" })]
    public async Task<ApiResult<PayRecordDto>> Charge(ChargeRequest req)
    {
        var result = await _service.ChargeAsync(req);
        return ApiResult<PayRecordDto>.Ok(result, "收费成功");
    }

    /// <summary>退费：回写状态并回补库存（事务）</summary>
    [HttpPost("refund")]
    [SwaggerOperation(
        Summary = "退费",
        Description = "回写收费单状态并回补已扣减的药品库存，全过程在同一事务内提交",
        Tags = new[] { "费用管理" })]
    public async Task<ApiResult<object>> Refund(RefundRequest req)
    {
        await _service.RefundAsync(req);
        return ApiResult.Ok("退费成功");
    }

    /// <summary>发药</summary>
    [HttpPost("dispense")]
    [SwaggerOperation(
        Summary = "发药",
        Description = "对已收费的处方执行发药，回写处方与收费单的发放状态",
        Tags = new[] { "费用管理" })]
    public async Task<ApiResult<object>> Dispense(DispenseRequest req)
    {
        await _service.DispenseAsync(req);
        return ApiResult.Ok("发药完成");
    }
}

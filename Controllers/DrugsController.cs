using His.Api.Dtos;
using His.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace His.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DrugsController : ControllerBase
{
    private readonly IDrugService _service;

    public DrugsController(IDrugService service)
    {
        _service = service;
    }

    /// <summary>分页查询药品</summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "分页查询药品",
        Description = "支持关键字模糊查询与分页，返回药品基本信息",
        Tags = new[] { "药品管理" })]
    public async Task<ApiResult<PageResult<DrugListItemDto>>> GetPaged([FromQuery] PageRequest req)
    {
        var result = await _service.GetPagedAsync(req);
        return ApiResult<PageResult<DrugListItemDto>>.Ok(result);
    }

    /// <summary>已启用药品下拉列表（含库存总量）</summary>
    [HttpGet("all")]
    [SwaggerOperation(
        Summary = "获取启用药品列表",
        Description = "返回已启用药品及其库存总量，供开处方时选择药品",
        Tags = new[] { "药品管理" })]
    public async Task<ApiResult<IReadOnlyList<DrugListItemDto>>> GetActiveList()
    {
        var result = await _service.GetActiveListAsync();
        return ApiResult<IReadOnlyList<DrugListItemDto>>.Ok(result);
    }

    /// <summary>新增药品</summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "新增药品",
        Description = "创建药品基本信息",
        Tags = new[] { "药品管理" })]
    public async Task<ApiResult<DrugListItemDto>> Create(DrugSaveRequest req)
    {
        var result = await _service.CreateAsync(req);
        return ApiResult<DrugListItemDto>.Ok(result, "新增成功");
    }

    /// <summary>修改药品信息</summary>
    [HttpPut("{id:long}")]
    [SwaggerOperation(
        Summary = "修改药品",
        Description = "按主键更新药品基本信息，不影响已有库存批次",
        Tags = new[] { "药品管理" })]
    public async Task<ApiResult<DrugListItemDto>> Update(long id, DrugSaveRequest req)
    {
        var result = await _service.UpdateAsync(id, req);
        return ApiResult<DrugListItemDto>.Ok(result, "保存成功");
    }

    /// <summary>删除药品</summary>
    [HttpDelete("{id:long}")]
    [SwaggerOperation(
        Summary = "删除药品",
        Description = "按主键删除药品；已被处方或库存引用时由业务层校验拦截",
        Tags = new[] { "药品管理" })]
    public async Task<ApiResult<object>> Delete(long id)
    {
        await _service.DeleteAsync(id);
        return ApiResult.Ok("删除成功");
    }

    /// <summary>药品库存批次列表</summary>
    [HttpGet("{id:long}/stocks")]
    [SwaggerOperation(
        Summary = "查询药品库存批次",
        Description = "返回指定药品的所有库存批次（含批号、有效期、剩余数量）",
        Tags = new[] { "药品管理" })]
    public async Task<ApiResult<IReadOnlyList<DrugStockDto>>> GetStocks(long id)
    {
        var result = await _service.GetStocksAsync(id);
        return ApiResult<IReadOnlyList<DrugStockDto>>.Ok(result);
    }

    /// <summary>入库（新增批次）</summary>
    [HttpPost("{id:long}/stocks")]
    [SwaggerOperation(
        Summary = "药品入库",
        Description = "为指定药品新增一个库存批次",
        Tags = new[] { "药品管理" })]
    public async Task<ApiResult<DrugStockDto>> AddStock(long id, StockAddRequest req)
    {
        var result = await _service.AddStockAsync(id, req);
        return ApiResult<DrugStockDto>.Ok(result, "入库成功");
    }

    /// <summary>调整库存批次</summary>
    [HttpPut("stocks/{stockId:long}")]
    [SwaggerOperation(
        Summary = "调整库存批次",
        Description = "按批次主键修改库存数量、批号、有效期等信息",
        Tags = new[] { "药品管理" })]
    public async Task<ApiResult<DrugStockDto>> UpdateStock(long stockId, StockUpdateRequest req)
    {
        var result = await _service.UpdateStockAsync(stockId, req);
        return ApiResult<DrugStockDto>.Ok(result, "库存已调整");
    }
}

using His.Api.Dtos;
using His.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace His.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[SwaggerTag("科室管理")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _service;

    public DepartmentsController(IDepartmentService service)
    {
        _service = service;
    }

    /// <summary>分页查询科室，关键字匹配科室名称</summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "分页查询科室",
        Description = "关键字匹配科室名称；先分页取实体，再在内存中拼装科室类型名称",
        Tags = new[] { "科室管理" })]
    public async Task<ApiResult<PageResult<DepartmentListItemDto>>> GetPaged([FromQuery] PageRequest req)
    {
        var result = await _service.GetPagedAsync(req);
        return ApiResult<PageResult<DepartmentListItemDto>>.Ok(result);
    }

    /// <summary>启用中的科室下拉列表</summary>
    [HttpGet("all")]
    [SwaggerOperation(
        Summary = "获取启用科室列表",
        Description = "返回所有启用状态（IsActive = 0）的科室，供挂号、医生等表单下拉选择",
        Tags = new[] { "科室管理" })]
    public async Task<ApiResult<IReadOnlyList<DepartmentListItemDto>>> GetAllActive()
    {
        var result = await _service.GetAllActiveAsync();
        return ApiResult<IReadOnlyList<DepartmentListItemDto>>.Ok(result);
    }

    /// <summary>科室类型字典（保留表达式体写法，用于和块体写法对照学习）</summary>
    [HttpGet("types")]
    [SwaggerOperation(
        Summary = "获取科室类型字典",
        Description = "返回科室类型字典数据，用于科室表单的类型下拉选择",
        Tags = new[] { "科室管理" })]
    public async Task<ApiResult<IReadOnlyList<DepartmentTypeDto>>> GetTypes()
        => ApiResult<IReadOnlyList<DepartmentTypeDto>>.Ok(await _service.GetTypesAsync());

    /// <summary>新增科室，名称唯一</summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "新增科室",
        Description = "科室名称不可重复；科室类型必须存在于类型字典中",
        Tags = new[] { "科室管理" })]
    public async Task<ApiResult<DepartmentListItemDto>> Create(DepartmentSaveRequest req)
    {
        var result = await _service.CreateAsync(req);
        return ApiResult<DepartmentListItemDto>.Ok(result, "新增成功");
    }

    /// <summary>修改科室</summary>
    [HttpPut("{id:int}")]
    [SwaggerOperation(
        Summary = "修改科室",
        Description = "按主键更新科室信息；名称不能与其他科室重复",
        Tags = new[] { "科室管理" })]
    public async Task<ApiResult<DepartmentListItemDto>> Update(int id, DepartmentSaveRequest req)
    {
        var result = await _service.UpdateAsync(id, req);
        return ApiResult<DepartmentListItemDto>.Ok(result, "保存成功");
    }

    /// <summary>删除科室，存在医生或挂号记录时拒绝</summary>
    [HttpDelete("{id:int}")]
    [SwaggerOperation(
        Summary = "删除科室",
        Description = "科室下存在医生或挂号记录时不允许删除",
        Tags = new[] { "科室管理" })]
    public async Task<ApiResult<object>> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return ApiResult.Ok("删除成功");
    }
}

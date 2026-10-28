using His.Api.Dtos;
using His.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace His.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _service;

    public DoctorsController(IDoctorService service)
    {
        _service = service;
    }

    /// <summary>分页查询医生，可按科室过滤</summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "分页查询医生",
        Description = "支持关键字与科室（deptId）过滤，支持分页",
        Tags = new[] { "医生管理" })]
    public async Task<ApiResult<PageResult<DoctorListItemDto>>> GetPaged([FromQuery] PageRequest req, [FromQuery] int? deptId)
    {
        var result = await _service.GetPagedAsync(req, deptId);
        return ApiResult<PageResult<DoctorListItemDto>>.Ok(result);
    }

    /// <summary>在职医生下拉列表，可按科室过滤</summary>
    [HttpGet("list")]
    [SwaggerOperation(
        Summary = "获取在职医生列表",
        Description = "返回在职医生，可按科室（deptId）过滤，供挂号等表单下拉选择",
        Tags = new[] { "医生管理" })]
    public async Task<ApiResult<IReadOnlyList<DoctorListItemDto>>> GetActiveList([FromQuery] int? deptId)
    {
        var result = await _service.GetActiveListAsync(deptId);
        return ApiResult<IReadOnlyList<DoctorListItemDto>>.Ok(result);
    }

    /// <summary>新增医生</summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "新增医生",
        Description = "创建医生档案，所属科室必须存在",
        Tags = new[] { "医生管理" })]
    public async Task<ApiResult<DoctorListItemDto>> Create(DoctorSaveRequest req)
    {
        var result = await _service.CreateAsync(req);
        return ApiResult<DoctorListItemDto>.Ok(result, "新增成功");
    }

    /// <summary>修改医生信息</summary>
    [HttpPut("{id:long}")]
    [SwaggerOperation(
        Summary = "修改医生",
        Description = "按主键更新医生档案信息",
        Tags = new[] { "医生管理" })]
    public async Task<ApiResult<DoctorListItemDto>> Update(long id, DoctorSaveRequest req)
    {
        var result = await _service.UpdateAsync(id, req);
        return ApiResult<DoctorListItemDto>.Ok(result, "保存成功");
    }

    /// <summary>删除医生</summary>
    [HttpDelete("{id:long}")]
    [SwaggerOperation(
        Summary = "删除医生",
        Description = "按主键删除医生；已被挂号或就诊引用时由业务层校验拦截",
        Tags = new[] { "医生管理" })]
    public async Task<ApiResult<object>> Delete(long id)
    {
        await _service.DeleteAsync(id);
        return ApiResult.Ok("删除成功");
    }
}

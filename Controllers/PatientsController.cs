using His.Api.Dtos;
using His.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace His.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _service;

    public PatientsController(IPatientService service)
    {
        _service = service;
    }

    /// <summary>分页查询患者，关键字匹配姓名/手机号/身份证号</summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "分页查询患者",
        Description = "关键字匹配患者姓名、手机号、身份证号，支持分页",
        Tags = new[] { "患者管理" })]
    public async Task<ApiResult<PageResult<PatientListItemDto>>> GetPaged([FromQuery] PageRequest req)
    {
        var result = await _service.GetPagedAsync(req);
        return ApiResult<PageResult<PatientListItemDto>>.Ok(result);
    }

    /// <summary>按主键查询患者详情</summary>
    [HttpGet("{id:long}")]
    [SwaggerOperation(
        Summary = "查询患者详情",
        Description = "按患者主键返回患者档案信息",
        Tags = new[] { "患者管理" })]
    public async Task<ApiResult<PatientListItemDto>> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id);
        return ApiResult<PatientListItemDto>.Ok(result);
    }

    /// <summary>新增患者</summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "新增患者",
        Description = "创建患者档案，身份证号若填写必须为 18 位",
        Tags = new[] { "患者管理" })]
    public async Task<ApiResult<PatientListItemDto>> Create(PatientSaveRequest req)
    {
        var result = await _service.CreateAsync(req);
        return ApiResult<PatientListItemDto>.Ok(result, "新增成功");
    }

    /// <summary>修改患者档案</summary>
    [HttpPut("{id:long}")]
    [SwaggerOperation(
        Summary = "修改患者",
        Description = "按主键更新患者档案信息",
        Tags = new[] { "患者管理" })]
    public async Task<ApiResult<PatientListItemDto>> Update(long id, PatientSaveRequest req)
    {
        var result = await _service.UpdateAsync(id, req);
        return ApiResult<PatientListItemDto>.Ok(result, "保存成功");
    }

    /// <summary>删除患者</summary>
    [HttpDelete("{id:long}")]
    [SwaggerOperation(
        Summary = "删除患者",
        Description = "按主键删除患者档案；已被挂号等业务引用时由业务层校验拦截",
        Tags = new[] { "患者管理" })]
    public async Task<ApiResult<object>> Delete(long id)
    {
        await _service.DeleteAsync(id);
        return ApiResult.Ok("删除成功");
    }
}

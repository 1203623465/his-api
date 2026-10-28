using His.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace His.Api.Controllers;

/// <summary>
/// 假登录：不做真实鉴权，账号密码非空即登录成功。
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    /// <summary>用户登录（演示用，不校验真实密码）</summary>
    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "用户登录",
        Description = "演示用假登录：账号、密码非空即返回固定的管理员身份，不做真实鉴权",
        Tags = new[] { "登录认证" })]
    public ApiResult<LoginUserDto> Login(LoginRequest request)
    {
        var user = new LoginUserDto(1, request.Username, "系统管理员", "admin");
        return ApiResult<LoginUserDto>.Ok(user, "登录成功");
    }
}

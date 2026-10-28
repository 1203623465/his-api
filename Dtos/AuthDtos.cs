using System.ComponentModel.DataAnnotations;

namespace His.Api.Dtos;

public class LoginRequest
{
    [Required(ErrorMessage = "账号不能为空")]
    [StringLength(50, ErrorMessage = "账号长度不能超过50")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "密码不能为空")]
    [StringLength(100, ErrorMessage = "密码长度不能超过100")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// 假登录返回的用户信息。
/// </summary>
public class LoginUserDto
{
    public long Id { get; set; }
    public string Username { get; set; }
    public string DisplayName { get; set; }
    public string Role { get; set; }

    public LoginUserDto(long id, string username, string displayName, string role)
    {
        Id = id;
        Username = username;
        DisplayName = displayName;
        Role = role;
    }
}

namespace His.Api.Dtos;

/// <summary>
/// 统一 API 响应结构。Code = 0 表示成功，非 0 表示业务错误。
/// </summary>
public class ApiResult<T>
{
    public int Code { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }

    public ApiResult(int code, string message, T? data)
    {
        Code = code;
        Message = message;
        Data = data;
    }

    public static ApiResult<T> Ok(T data, string message = "success") => new(0, message, data);

    public static ApiResult<T> Fail(int code, string message) => new(code, message, default);
}

public static class ApiResult
{
    public static ApiResult<object> Ok(string message = "success") => new(0, message, null);
}

using System.Net;
using System.Text.Json;
using His.Api.Dtos;
using Serilog.Context;

namespace His.Api.Middleware;

/// <summary>
/// 业务异常，抛出后由异常中间件转换为统一响应。
/// </summary>
public class ApiException : Exception
{
    public int Code { get; }

    public ApiException(string message, int code = 400) : base(message)
    {
        Code = code;
    }
}

/// <summary>
/// 全局异常中间件：业务异常返回友好错误，未知异常记录日志并隐藏堆栈。
/// </summary>
public class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 统一追踪标识：把 ASP.NET Core 的请求 ID 注入 Serilog 上下文，
        // 使文件日志模板里的 {RequestId} 与异常日志中的 RequestId 完全一致
        using (LogContext.PushProperty("RequestId", context.TraceIdentifier))
        {
            try
            {
                await _next(context);
            }
            catch (ApiException ex)
            {
                _logger.LogWarning("业务异常: {Message}", ex.Message);
                await WriteResultAsync(context, HttpStatusCode.OK, ex.Code, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "未处理异常: {Method} {Path} RequestId={RequestId}",
                    context.Request.Method, context.Request.Path, context.TraceIdentifier);
                await WriteResultAsync(context, HttpStatusCode.InternalServerError, 500, "服务器内部错误，请稍后再试");
            }
        }
    }

    private static async Task WriteResultAsync(HttpContext context, HttpStatusCode status, int code, string message)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json; charset=utf-8";
        var json = JsonSerializer.Serialize(ApiResult<object>.Fail(code, message), JsonOptions);
        await context.Response.WriteAsync(json);
    }
}

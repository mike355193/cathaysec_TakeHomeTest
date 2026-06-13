using System.Text.Json;
using CathaySec.Api.Application;
using CathaySec.Api.Logging;
using CathaySec.Api.Models;

namespace CathaySec.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    IStructuredLogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var (statusCode, code, message) = exception switch
            {
                ResourceNotFoundException => (StatusCodes.Status404NotFound, "NOT_FOUND", exception.Message),
                BusinessRuleException => (StatusCodes.Status400BadRequest, "BUSINESS_RULE_VIOLATION", exception.Message),
                UpstreamServiceException => (StatusCodes.Status503ServiceUnavailable, "UPSTREAM_UNAVAILABLE", exception.Message),
                _ => (StatusCodes.Status500InternalServerError, "INTERNAL_ERROR", "An unexpected error occurred.")
            };

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                logger.Log(
                    LogLevel.Error,
                    "發生未處理的系統例外。",
                    LogTag.LogCatch,
                    new Dictionary<string, object?>
                    {
                        ["TraceId"] = context.TraceIdentifier,
                        ["Method"] = context.Request.Method,
                        ["Path"] = context.Request.Path.Value
                    },
                    code,
                    exception);
            }
            else
            {
                logger.Log(
                    LogLevel.Warning,
                    "請求因可預期的應用程式錯誤而失敗。",
                    exception is BusinessRuleException ? LogTag.Validation : LogTag.Usual,
                    new Dictionary<string, object?>
                    {
                        ["TraceId"] = context.TraceIdentifier,
                        ["Method"] = context.Request.Method,
                        ["Path"] = context.Request.Path.Value,
                        ["Reason"] = exception.Message
                    },
                    code);
            }

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            var response = ApiResponse<object>.Failure(code, message).WithTraceId(context.TraceIdentifier);
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
        }
    }
}

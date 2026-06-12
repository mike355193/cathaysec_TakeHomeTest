using System.Diagnostics;
using CathaySec.Api.Logging;

namespace CathaySec.Api.Middleware;

public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    IStructuredLogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            var logLevel = context.Response.StatusCode >= StatusCodes.Status500InternalServerError
                ? LogLevel.Error
                : context.Response.StatusCode >= StatusCodes.Status400BadRequest
                    ? LogLevel.Warning
                    : LogLevel.Information;

            logger.Log(
                logLevel,
                "HTTP 請求處理完成。",
                LogTag.Request,
                new Dictionary<string, object?>
                {
                    ["TraceId"] = context.TraceIdentifier,
                    ["Method"] = context.Request.Method,
                    ["Path"] = context.Request.Path.Value,
                    ["StatusCode"] = context.Response.StatusCode,
                    ["ElapsedMs"] = stopwatch.Elapsed.TotalMilliseconds,
                    ["ClientIp"] = context.Connection.RemoteIpAddress?.ToString()
                });
        }
    }
}

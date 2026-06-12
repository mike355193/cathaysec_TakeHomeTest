using CathaySec.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CathaySec.Api.Middleware;

public sealed class ApiResponseFilter : IAsyncResultFilter
{
    public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult { Value: not null } result &&
            result.Value.GetType().IsGenericType &&
            result.Value.GetType().GetGenericTypeDefinition() == typeof(ApiResponse<>))
        {
            return next();
        }

        if (context.Result is ObjectResult objectResult)
        {
            context.Result = new ObjectResult(ApiResponse<object>.Ok(objectResult.Value!)
                .WithTraceId(context.HttpContext.TraceIdentifier))
            {
                StatusCode = objectResult.StatusCode
            };
        }

        return next();
    }
}

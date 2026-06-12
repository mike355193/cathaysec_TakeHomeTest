using CathaySec.Api.Models;
using Swashbuckle.AspNetCore.Filters;

namespace CathaySec.Api.Swagger.Examples;

public sealed class StockSearchResponseExample : IExamplesProvider<ApiResponse<PagedResult<StockResponse>>>
{
    public ApiResponse<PagedResult<StockResponse>> GetExamples() => new(
        true,
        new PagedResult<StockResponse>(
            [
                new("2330", "（台積電）台灣積體電路製造股份有限公司"),
                new("2317", "（鴻海）鴻海精密工業股份有限公司")
            ],
            1,
            20,
            2),
        null,
        "0HNDAMPLE0001");
}

public sealed class StockDetailResponseExample : IExamplesProvider<ApiResponse<StockResponse>>
{
    public ApiResponse<StockResponse> GetExamples() => new(
        true,
        new StockResponse(
            "2330",
            "（台積電）台灣積體電路製造股份有限公司"),
        null,
        "0HNDAMPLE0002");
}

public sealed class StockNotFoundResponseExample : IExamplesProvider<ApiResponse<object>>
{
    public ApiResponse<object> GetExamples() => new(
        false,
        null,
        new ApiError("NOT_FOUND", "Stock '9999' was not found."),
        "0HNDAMPLE0003");
}

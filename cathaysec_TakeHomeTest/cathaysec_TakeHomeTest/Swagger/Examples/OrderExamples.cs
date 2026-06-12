using CathaySec.Api.Domain;
using CathaySec.Api.Models;
using Swashbuckle.AspNetCore.Filters;

namespace CathaySec.Api.Swagger.Examples;

public sealed class CreateOrderRequestExample : IExamplesProvider<CreateOrderRequest>
{
    public CreateOrderRequest GetExamples() => new()
    {
        Symbol = "2330",
        Side = OrderSide.Buy,
        Price = 1045.00m,
        Quantity = 1000
    };
}

public sealed class CreateOrderResponseExample : IExamplesProvider<ApiResponse<OrderResponse>>
{
    public ApiResponse<OrderResponse> GetExamples() => OrderExampleFactory.Success("0HNDAMPLE0004");
}

public sealed class GetOrderResponseExample : IExamplesProvider<ApiResponse<OrderResponse>>
{
    public ApiResponse<OrderResponse> GetExamples() => OrderExampleFactory.Success("0HNDAMPLE0005");
}

public sealed class OrderValidationErrorResponseExample : IExamplesProvider<ApiResponse<object>>
{
    public ApiResponse<object> GetExamples() => new(
        false,
        null,
        new ApiError(
            "VALIDATION_ERROR",
            "Request validation failed.",
            new Dictionary<string, string[]>
            {
                ["Price"] = ["委託價格必須大於 0。"]
            }),
        "0HNDAMPLE0006");
}

public sealed class OrderNotFoundResponseExample : IExamplesProvider<ApiResponse<object>>
{
    public ApiResponse<object> GetExamples() => new(
        false,
        null,
        new ApiError("NOT_FOUND", "Order '11111111-1111-1111-1111-111111111111' was not found."),
        "0HNDAMPLE0007");
}

internal static class OrderExampleFactory
{
    private static readonly Guid ExampleOrderId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static ApiResponse<OrderResponse> Success(string traceId) => new(
        true,
        new OrderResponse(
            ExampleOrderId,
            "2330",
            OrderSide.Buy,
            1045.00m,
            1000,
            OrderStatus.Pending,
            new DateTimeOffset(2026, 6, 12, 13, 30, 0, TimeSpan.FromHours(8))),
        null,
        traceId);
}

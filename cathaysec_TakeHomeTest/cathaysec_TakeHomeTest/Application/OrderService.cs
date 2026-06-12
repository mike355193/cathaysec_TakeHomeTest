using CathaySec.Api.Domain;
using CathaySec.Api.Logging;
using CathaySec.Api.Models;

namespace CathaySec.Api.Application;

public sealed class OrderService(
    IOrderRepository orders,
    IStockRepository stocks,
    TimeProvider timeProvider,
    IStructuredLogger<OrderService> logger) : IOrderService
{
    public async Task<OrderResponse> CreateAsync(
        CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var symbol = request.Symbol.Trim().ToUpperInvariant();
        if (await stocks.GetBySymbolAsync(symbol, cancellationToken) is null)
        {
            throw new BusinessRuleException($"Cannot create an order for unknown stock '{symbol}'.");
        }

        var order = new Order(
            Guid.NewGuid(), symbol, request.Side, request.Price, request.Quantity,
            OrderStatus.Pending, timeProvider.GetUtcNow());

        await orders.AddAsync(order, cancellationToken);
        logger.Log(
            LogLevel.Information,
            "委託單建立成功。",
            LogTag.Order,
            new Dictionary<string, object?>
            {
                ["OrderId"] = order.Id,
                ["Symbol"] = order.Symbol,
                ["Side"] = order.Side,
                ["Quantity"] = order.Quantity,
                ["Price"] = order.Price
            });

        return Map(order);
    }

    public async Task<OrderResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Order '{id}' was not found.");
        return Map(order);
    }

    private static OrderResponse Map(Order order) => new(
        order.Id, order.Symbol, order.Side, order.Price, order.Quantity, order.Status, order.CreatedAt);
}

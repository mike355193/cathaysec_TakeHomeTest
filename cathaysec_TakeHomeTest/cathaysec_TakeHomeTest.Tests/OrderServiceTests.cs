using CathaySec.Api.Application;
using CathaySec.Api.Domain;
using CathaySec.Api.Infrastructure;
using CathaySec.Api.Logging;
using CathaySec.Api.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace CathaySec.Api.Tests;

public sealed class OrderServiceTests
{
    /// <summary>
    /// 驗證使用已存在的股票建立委託時，會儲存並回傳狀態為等待處理的委託單。
    /// </summary>
    [Fact]
    public async Task CreateAsync_KnownStock_PersistsPendingOrder()
    {
        var repository = new InMemoryOrderRepository();
        var service = CreateService(repository);
        var request = new CreateOrderRequest
        {
            Symbol = "2330",
            Side = OrderSide.Buy,
            Price = 1000m,
            Quantity = 1000
        };

        var created = await service.CreateAsync(request, CancellationToken.None);
        var found = await service.GetByIdAsync(created.Id, CancellationToken.None);

        Assert.Equal(OrderStatus.Pending, created.Status);
        Assert.Equal(created, found);
    }

    /// <summary>
    /// 驗證使用不存在的股票建立委託時，會拋出商業規則例外且不建立委託單。
    /// </summary>
    [Fact]
    public async Task CreateAsync_UnknownStock_ThrowsBusinessRuleException()
    {
        var service = CreateService(new InMemoryOrderRepository());
        var request = new CreateOrderRequest
        {
            Symbol = "9999",
            Side = OrderSide.Sell,
            Price = 10m,
            Quantity = 1
        };

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.CreateAsync(request, CancellationToken.None));
    }

    private static OrderService CreateService(IOrderRepository repository) => new(
        repository,
        new InMemoryStockRepository(
            [new Stock("2330", "（台積電）台灣積體電路製造股份有限公司")]),
        TimeProvider.System,
        new NullStructuredLogger<OrderService>());
}

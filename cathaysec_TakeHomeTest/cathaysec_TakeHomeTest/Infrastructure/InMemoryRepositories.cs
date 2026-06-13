using System.Collections.Concurrent;
using CathaySec.Api.Application;
using CathaySec.Api.Domain;

namespace CathaySec.Api.Infrastructure;

public sealed class InMemoryStockRepository : IStockRepository
{
    private Stock[] _stocks;

    public InMemoryStockRepository(IEnumerable<Stock>? stocks = null)
    {
        _stocks = stocks?.OrderBy(stock => stock.Symbol).ToArray() ?? [];
    }

    public void ReplaceAll(IEnumerable<Stock> stocks)
    {
        _stocks = stocks.OrderBy(stock => stock.Symbol).ToArray();
    }

    public Task<(IReadOnlyList<Stock> Items, int TotalCount)> SearchAsync(
        string? symbol, string? keyword, int page, int pageSize, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IEnumerable<Stock> query = _stocks;

        if (!string.IsNullOrWhiteSpace(symbol))
        {
            query = query.Where(stock => stock.Symbol.Contains(symbol, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(stock =>
                stock.Symbol.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                stock.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        var filtered = query.OrderBy(stock => stock.Symbol).ToArray();
        var offset = ((long)page - 1) * pageSize;
        IReadOnlyList<Stock> pageItems = offset >= filtered.Length
            ? []
            : filtered.Skip((int)offset).Take(pageSize).ToArray();
        return Task.FromResult((pageItems, filtered.Length));
    }

    public Task<Stock?> GetBySymbolAsync(string symbol, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_stocks.FirstOrDefault(stock =>
            stock.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase)));
    }
}

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();

    public Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_orders.TryAdd(order.Id, order))
        {
            throw new InvalidOperationException($"Order '{order.Id}' already exists.");
        }

        return Task.CompletedTask;
    }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _orders.TryGetValue(id, out var order);
        return Task.FromResult(order);
    }
}

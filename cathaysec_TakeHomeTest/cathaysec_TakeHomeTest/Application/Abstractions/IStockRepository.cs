using CathaySec.Api.Domain;

namespace CathaySec.Api.Application;

public interface IStockRepository
{
    Task<(IReadOnlyList<Stock> Items, int TotalCount)> SearchAsync(
        string? symbol, string? keyword, int page, int pageSize, CancellationToken cancellationToken);

    Task<Stock?> GetBySymbolAsync(string symbol, CancellationToken cancellationToken);
}

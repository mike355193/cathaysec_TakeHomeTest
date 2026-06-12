using CathaySec.Api.Models;

namespace CathaySec.Api.Application;

public interface IStockService
{
    Task<PagedResult<StockResponse>> SearchAsync(StockQuery query, CancellationToken cancellationToken);
    Task<StockResponse> GetBySymbolAsync(string symbol, CancellationToken cancellationToken);
}

using CathaySec.Api.Models;

namespace CathaySec.Api.Application;

public interface IStockService
{
    Task<PagedResult<StockResponse>> SearchAsync(StockQuery query, CancellationToken cancellationToken);
    Task<StockDetailResponse> GetBySymbolAsync(string symbol, CancellationToken cancellationToken);
}

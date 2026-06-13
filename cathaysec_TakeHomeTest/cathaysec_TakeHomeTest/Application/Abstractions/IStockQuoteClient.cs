using CathaySec.Api.Domain;

namespace CathaySec.Api.Application;

public interface IStockQuoteClient
{
    Task<StockQuote?> GetAsync(string symbol, CancellationToken cancellationToken);
}

using CathaySec.Api.Models;
using Microsoft.Extensions.Caching.Memory;

namespace CathaySec.Api.Application;

public sealed class StockService(
    IStockRepository stocks,
    IStockQuoteClient quotes,
    IMemoryCache cache) : IStockService
{
    public async Task<PagedResult<StockResponse>> SearchAsync(
        StockQuery query, CancellationToken cancellationToken)
    {
        var symbol = query.Symbol?.Trim();
        var keyword = query.Keyword?.Trim();
        var cacheKey = $"stock-search:{symbol}:{keyword}:{query.Page}:{query.PageSize}";

        return await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
            var (items, totalCount) = await stocks.SearchAsync(
                symbol, keyword, query.Page, query.PageSize, cancellationToken);

            return new PagedResult<StockResponse>(
                items.Select(stock => new StockResponse(stock.Symbol, stock.Name)).ToArray(),
                query.Page,
                query.PageSize,
                totalCount);
        }) ?? new PagedResult<StockResponse>([], query.Page, query.PageSize, 0);
    }

    public async Task<StockDetailResponse> GetBySymbolAsync(
        string symbol, CancellationToken cancellationToken)
    {
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();
        var stock = await stocks.GetBySymbolAsync(normalizedSymbol, cancellationToken);

        if (stock is null)
        {
            throw new ResourceNotFoundException($"Stock '{normalizedSymbol}' was not found.");
        }

        var quote = await cache.GetOrCreateAsync(
            $"stock-quote:{normalizedSymbol}",
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5);
                return await quotes.GetAsync(normalizedSymbol, cancellationToken);
            });

        return new StockDetailResponse(
            stock.Symbol,
            stock.Name,
            quote?.LastPrice,
            quote?.PreviousClose,
            quote?.OpenPrice,
            quote?.HighPrice,
            quote?.LowPrice,
            quote?.AccumulatedVolume,
            quote?.QuotedAt);
    }
}

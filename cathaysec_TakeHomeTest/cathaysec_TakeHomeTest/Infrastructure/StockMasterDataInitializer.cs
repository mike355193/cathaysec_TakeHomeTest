using CathaySec.Api.Application;
using CathaySec.Api.Domain;
using CathaySec.Api.Logging;

namespace CathaySec.Api.Infrastructure;

public sealed class StockMasterDataInitializer(
    IStockMasterDataClient client,
    InMemoryStockRepository repository,
    IStructuredLogger<StockMasterDataInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<Stock> stocks = [];
        Exception? lastException = null;

        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                stocks = await client.GetListedStocksAsync(cancellationToken);
                if (stocks.Count > 0)
                {
                    break;
                }
            }
            catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
            {
                lastException = exception;
                logger.Log(
                    LogLevel.Warning,
                    "上市公司股票主檔載入失敗，準備重試。",
                    LogTag.Stock,
                    new Dictionary<string, object?> { ["Attempt"] = attempt },
                    "TWSE_STOCK_MASTER_LOAD_FAILED",
                    exception);
            }

            if (attempt < 3)
            {
                await Task.Delay(TimeSpan.FromSeconds(attempt), cancellationToken);
            }
        }

        if (stocks.Count == 0)
        {
            throw new InvalidOperationException("TWSE stock master data could not be loaded.", lastException);
        }

        repository.ReplaceAll(stocks);
        logger.Log(
            LogLevel.Information,
            "上市公司股票主檔載入完成。",
            LogTag.Stock,
            new Dictionary<string, object?> { ["StockCount"] = stocks.Count });
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

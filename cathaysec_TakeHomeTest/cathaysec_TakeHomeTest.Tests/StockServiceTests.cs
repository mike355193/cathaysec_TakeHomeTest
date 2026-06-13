using CathaySec.Api.Application;
using CathaySec.Api.Domain;
using CathaySec.Api.Infrastructure;
using CathaySec.Api.Models;
using Microsoft.Extensions.Caching.Memory;

namespace CathaySec.Api.Tests;

public sealed class StockServiceTests
{
    /// <summary>
    /// 驗證股票分頁查詢會回傳指定頁碼、每頁筆數、資料總筆數與正確總頁數。
    /// </summary>
    [Fact]
    public async Task SearchAsync_ReturnsRequestedPageAndTotalCount()
    {
        var service = CreateService();

        var result = await service.SearchAsync(
            new StockQuery { Page = 2, PageSize = 5 }, CancellationToken.None);

        Assert.Equal(2, result.Page);
        Assert.Equal(5, result.Items.Count);
        Assert.Equal(12, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
    }

    /// <summary>
    /// 驗證使用不存在的股票代號查詢明細時，會拋出資源不存在例外。
    /// </summary>
    [Fact]
    public async Task GetBySymbolAsync_UnknownSymbol_ThrowsNotFound()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
            service.GetBySymbolAsync("9999", CancellationToken.None));
    }

    /// <summary>
    /// 驗證單筆股票查詢會回傳由上市公司主檔載入至 InMemory 的股票資料。
    /// </summary>
    [Fact]
    public async Task GetBySymbolAsync_KnownSymbol_ReturnsStockMasterData()
    {
        var service = CreateService();

        var result = await service.GetBySymbolAsync("0050", CancellationToken.None);

        Assert.Equal("0050", result.Symbol);
        Assert.Equal("（元大台灣50）元大台灣卓越50證券投資信託基金", result.Name);
    }

    [Fact]
    public async Task GetBySymbolAsync_RepeatedRequest_UsesQuoteCache()
    {
        var quoteClient = new FakeStockQuoteClient(new StockQuote(
            1000m, 990m, 995m, 1005m, 985m, 12345,
            new DateTimeOffset(2026, 6, 12, 5, 30, 0, TimeSpan.Zero)));
        var service = CreateService(quoteClient);

        var first = await service.GetBySymbolAsync("2330", CancellationToken.None);
        var second = await service.GetBySymbolAsync("2330", CancellationToken.None);

        Assert.Equal(1000m, first.LastPrice);
        Assert.Equal(first, second);
        Assert.Equal(1, quoteClient.CallCount);
    }

    [Fact]
    public async Task SearchAsync_VeryLargePage_ReturnsEmptyPageWithoutOverflow()
    {
        var service = CreateService();

        var result = await service.SearchAsync(
            new StockQuery { Page = int.MaxValue, PageSize = 100 }, CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(12, result.TotalCount);
    }

    private static StockService CreateService(IStockQuoteClient? quoteClient = null) => new(
        new InMemoryStockRepository(TestStocks),
        quoteClient ?? new FakeStockQuoteClient(null),
        new MemoryCache(new MemoryCacheOptions()));

    private sealed class FakeStockQuoteClient(StockQuote? quote) : IStockQuoteClient
    {
        public int CallCount { get; private set; }

        public Task<StockQuote?> GetAsync(string symbol, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(quote);
        }
    }

    private static readonly Stock[] TestStocks =
    [
        new("0050", "（元大台灣50）元大台灣卓越50證券投資信託基金"),
        new("1101", "（台泥）臺灣水泥股份有限公司"),
        new("1216", "（統一）統一企業股份有限公司"),
        new("1301", "（台塑）台灣塑膠工業股份有限公司"),
        new("2002", "（中鋼）中國鋼鐵股份有限公司"),
        new("2303", "（聯電）聯華電子股份有限公司"),
        new("2317", "（鴻海）鴻海精密工業股份有限公司"),
        new("2330", "（台積電）台灣積體電路製造股份有限公司"),
        new("2454", "（聯發科）聯發科技股份有限公司"),
        new("2881", "（富邦金）富邦金融控股股份有限公司"),
        new("2882", "（國泰金）國泰金融控股股份有限公司"),
        new("6505", "（台塑化）台塑石化股份有限公司")
    ];
}

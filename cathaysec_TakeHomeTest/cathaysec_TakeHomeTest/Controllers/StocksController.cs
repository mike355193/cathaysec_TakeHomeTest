using System.ComponentModel.DataAnnotations;
using CathaySec.Api.Application;
using CathaySec.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using CathaySec.Api.Swagger.Examples;

namespace CathaySec.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/stocks")]
public sealed class StocksController(IStockService stocks) : ControllerBase
{
    /// <summary>
    /// 分頁查詢股票列表。
    /// </summary>
    /// <remarks>
    /// 依股票代號或關鍵字篩選股票，並以分頁方式回傳結果。
    /// </remarks>
    /// <param name="query">股票代號、關鍵字、頁碼與每頁筆數等查詢條件。</param>
    /// <param name="cancellationToken">HTTP 請求取消權杖。</param>
    /// <returns>符合條件的股票分頁資料。</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<StockResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(StockSearchResponseExample))]
    public async Task<ActionResult<PagedResult<StockResponse>>> Search(
        [FromQuery] StockQuery query, CancellationToken cancellationToken) =>
        Ok(await stocks.SearchAsync(query, cancellationToken));

    /// <summary>
    /// 依股票代號查詢上市公司股票主檔。
    /// </summary>
    /// <remarks>
    /// 依股票代號查詢應用程式啟動時由臺灣證券交易所 OpenAPI 載入的公司主檔資料。
    /// </remarks>
    /// <param name="symbol">股票代號，例如 2330。</param>
    /// <param name="cancellationToken">HTTP 請求取消權杖。</param>
    /// <returns>股票代號及公司名稱。</returns>
    [HttpGet("{symbol}")]
    [ProducesResponseType(typeof(ApiResponse<StockResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(StockDetailResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(StockNotFoundResponseExample))]
    public async Task<ActionResult<StockResponse>> GetBySymbol(
        [RegularExpression("^[0-9A-Za-z]{1,10}$")] string symbol,
        CancellationToken cancellationToken) =>
        Ok(await stocks.GetBySymbolAsync(symbol, cancellationToken));
}

using System.ComponentModel.DataAnnotations;
using CathaySec.Api.Domain;

namespace CathaySec.Api.Models;

public sealed class StockQuery
{
    /// <summary>
    /// 股票代號，可輸入完整或部分代號，例如 2330。
    /// </summary>
    /// <example>2330</example>
    [StringLength(10)]
    public string? Symbol { get; init; }

    /// <summary>
    /// 搜尋關鍵字，可比對股票代號或中文名稱，例如 台灣、台積電。
    /// </summary>
    /// <example>台灣</example>
    [StringLength(50)]
    public string? Keyword { get; init; }

    /// <summary>
    /// 頁碼，從 1 開始。
    /// </summary>
    /// <example>1</example>
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    /// <summary>
    /// 每頁筆數，最少 1 筆，最多 100 筆。
    /// </summary>
    /// <example>20</example>
    [Range(1, 100)]
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// 股票資料回應。
/// </summary>
/// <param name="Symbol">股票代號。</param>
/// <param name="Name">股票名稱，格式為（公司簡稱）公司名稱。</param>
public sealed record StockResponse(
    string Symbol,
    string Name);

public sealed class CreateOrderRequest
{
    /// <summary>
    /// 股票代號，例如 2330。
    /// </summary>
    /// <example>2330</example>
    [Required, RegularExpression("^[0-9A-Za-z]{1,10}$")]
    public required string Symbol { get; init; }

    /// <summary>
    /// 買賣方向，Buy 代表買進，Sell 代表賣出。
    /// </summary>
    /// <example>Buy</example>
    [EnumDataType(typeof(OrderSide))]
    public OrderSide Side { get; init; }

    /// <summary>
    /// 委託價格，必須大於 0。
    /// </summary>
    /// <example>1045.00</example>
    [Range(typeof(decimal), "0.01", "10000000")]
    public decimal Price { get; init; }

    /// <summary>
    /// 委託股數，必須為正整數。
    /// </summary>
    /// <example>1000</example>
    [Range(1, 100000000)]
    public int Quantity { get; init; }
}

/// <summary>
/// 委託單資料回應。
/// </summary>
/// <param name="Id">委託單唯一識別碼。</param>
/// <param name="Symbol">股票代號。</param>
/// <param name="Side">買賣方向。</param>
/// <param name="Price">委託價格。</param>
/// <param name="Quantity">委託股數。</param>
/// <param name="Status">委託狀態。</param>
/// <param name="CreatedAt">委託建立時間。</param>
public sealed record OrderResponse(
    Guid Id,
    string Symbol,
    OrderSide Side,
    decimal Price,
    int Quantity,
    OrderStatus Status,
    DateTimeOffset CreatedAt);

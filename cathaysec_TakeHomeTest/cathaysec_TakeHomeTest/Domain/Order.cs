namespace CathaySec.Api.Domain;

/// <summary>
/// 委託買賣方向。
/// </summary>
public enum OrderSide
{
    /// <summary>
    /// 買進。
    /// </summary>
    Buy,

    /// <summary>
    /// 賣出。
    /// </summary>
    Sell
}

/// <summary>
/// 委託單狀態。
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// 等待處理。
    /// </summary>
    Pending,

    /// <summary>
    /// 已成交。
    /// </summary>
    Filled,

    /// <summary>
    /// 已取消。
    /// </summary>
    Cancelled,

    /// <summary>
    /// 已拒絕。
    /// </summary>
    Rejected
}

public sealed record Order(
    Guid Id,
    string Symbol,
    OrderSide Side,
    decimal Price,
    int Quantity,
    OrderStatus Status,
    DateTimeOffset CreatedAt);

namespace CathaySec.Api.Models;

/// <summary>
/// API 錯誤內容。
/// </summary>
/// <param name="Code">系統錯誤代碼。</param>
/// <param name="Message">錯誤訊息。</param>
/// <param name="Details">錯誤明細，例如欄位驗證失敗原因。</param>
public sealed record ApiError(string Code, string Message, object? Details = null);

/// <summary>
/// API 統一回傳格式。
/// </summary>
/// <typeparam name="T">成功時回傳的資料型別。</typeparam>
/// <param name="Success">是否執行成功。</param>
/// <param name="Data">成功時的回傳資料；失敗時為 null。</param>
/// <param name="Error">失敗時的錯誤內容；成功時為 null。</param>
/// <param name="TraceId">本次請求的追蹤識別碼。</param>
public sealed record ApiResponse<T>(bool Success, T? Data, ApiError? Error, string TraceId)
{
    public static ApiResponse<T> Ok(T data) => new(true, data, null, string.Empty);

    public static ApiResponse<T> Failure(string code, string message, object? details = null) =>
        new(false, default, new ApiError(code, message, details), string.Empty);

    public ApiResponse<T> WithTraceId(string traceId) => this with { TraceId = traceId };
}

/// <summary>
/// 分頁查詢結果。
/// </summary>
/// <typeparam name="T">分頁項目的資料型別。</typeparam>
/// <param name="Items">本頁資料列表。</param>
/// <param name="Page">目前頁碼。</param>
/// <param name="PageSize">每頁筆數。</param>
/// <param name="TotalCount">符合條件的資料總筆數。</param>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    /// <summary>
    /// 總頁數。
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

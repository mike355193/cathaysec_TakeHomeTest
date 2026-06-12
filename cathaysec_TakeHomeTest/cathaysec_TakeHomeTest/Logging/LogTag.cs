namespace CathaySec.Api.Logging;

/// <summary>
/// 系統日誌的服務分類標籤。
/// </summary>
public enum LogTag
{
    /// <summary>
    /// 系統層級的一般事件，例如系統狀態或基礎元件操作。
    /// </summary>
    System,

    /// <summary>
    /// 系統自動使用：輸入資料、Model 或商業規則驗證失敗。
    /// </summary>
    Validation,

    /// <summary>
    /// 系統無法判斷事件分類時使用；應優先選擇更明確的標籤。
    /// </summary>
    Unknown,

    /// <summary>
    /// 系統自動使用：由全域例外處理或日誌捕捉機制記錄的未處理錯誤。
    /// </summary>
    LogCatch,

    /// <summary>
    /// 無特定業務分類的一般用途日誌。
    /// </summary>
    Usual,

    /// <summary>
    /// HTTP 請求生命週期相關，例如路徑、狀態碼與處理時間。
    /// </summary>
    Request,

    /// <summary>
    /// 股票清單、股票明細與股票搜尋相關操作。
    /// </summary>
    Stock,

    /// <summary>
    /// 委託單建立、查詢與狀態處理相關操作。
    /// </summary>
    Order,

    /// <summary>
    /// 身分驗證與授權相關，例如 API Key 驗證失敗。
    /// </summary>
    Authentication,

    /// <summary>
    /// 快取讀取、寫入、失效與命中狀態相關操作。
    /// </summary>
    Cache,

    /// <summary>
    /// 效能與延遲監控相關，例如處理時間超過預期門檻。
    /// </summary>
    Performance,

    /// <summary>
    /// 應用程式啟動、初始化與關閉流程相關事件。
    /// </summary>
    Startup
}

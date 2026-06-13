# 日誌規格

系統使用 NLog 與 `IStructuredLogger<T>`，所有應用程式日誌採固定 JSON schema。

| 欄位 | 型別 | 說明 |
| --- | --- | --- |
| `timestamp` | string | UTC ISO 8601 時間。 |
| `level` | string | `INFO`、`WARN`、`ERROR` 等級。 |
| `application` | string | 固定為 `CathaySec.Api`。 |
| `environmentName` | string | ASP.NET Core 環境名稱。 |
| `logger` | string | 寫入日誌的類別名稱。 |
| `message` | string | 人類可讀訊息。 |
| `traceId` | string | HTTP 請求追蹤碼；非 HTTP 事件可為空。 |
| `callSite` | string | `專案.類別.方法:行號`，例如 `cathaysec_TakeHomeTest.RequestLoggingMiddleware.InvokeAsync:26`。由 compiler caller attributes 自動產生。 |
| `serviceTag` | string | `LogTag` enum 的分類值。 |
| `errorCode` | string | 穩定的系統錯誤碼；一般事件為 `-`。 |
| `parameters` | object | 經過限制與遮罩的結構化參數。 |
| `exception` | string | 例外完整內容；無例外時為空。 |

## 參數安全

- 呼叫端使用 `IReadOnlyDictionary<string, object?>`，不接受無限制的 `dynamic` 物件反射。
- 最多記錄 20 個欄位或集合項目。
- 字串最多 500 字。
- 欄位名稱包含 `password`、`secret`、`token`、`authorization`、`apiKey` 或 `cookie` 時自動輸出 `***REDACTED***`。
- API request body 與 API Key 原值不得寫入日誌。

## 使用範例

```csharp
logger.Log(
    LogLevel.Information,
    "委託單建立成功。",
    LogTag.Order,
    new Dictionary<string, object?>
    {
        ["OrderId"] = order.Id,
        ["Symbol"] = order.Symbol,
        ["Side"] = order.Side,
        ["Quantity"] = order.Quantity,
        ["Price"] = order.Price
    });
```

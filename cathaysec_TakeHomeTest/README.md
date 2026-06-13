# 證券交易資料查詢系統 API

本專案使用 .NET 10 與 C# 建立 Controller-based RESTful API，提供股票查詢、TWSE 即時報價、建立委託單及委託查詢功能。股票與委託資料依題目要求使用 InMemory 儲存。

## 系統需求

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- 選用：[Docker Desktop](https://www.docker.com/products/docker-desktop/)
- 執行壓力測試時需要：[Grafana k6](https://grafana.com/docs/k6/latest/set-up/install-k6/)

## 啟動專案

在 solution 根目錄執行：

```powershell
dotnet restore .\cathaysec_TakeHomeTest.slnx
dotnet run --project .\cathaysec_TakeHomeTest\cathaysec_TakeHomeTest.csproj
```

啟動後可開啟：

- Swagger UI：`http://localhost:5155/swagger`
- Health Check：`http://localhost:5155/health`

所有 `/api/v1` API 都需要在 HTTP Header 帶入開發用 API Key：

```http
X-API-Key: cathaysec-dev-key
```

此金鑰僅供本機開發使用。正式環境不應將金鑰提交至版本控制，應透過 Secret Manager 或正式的機密管理服務設定 `Authentication__ApiKey`。

## Docker Compose

Visual Studio Docker Compose 僅啟動 Web API，預設 Swagger 位址為：

```text
https://localhost:612/swagger
```

可使用下列命令啟動：

```powershell
docker compose up --build
```

## API 端點

| Method | Endpoint | 說明 |
|---|---|---|
| `GET` | `/api/v1/stocks?symbol=&keyword=&page=1&pageSize=20` | 股票條件搜尋與分頁查詢 |
| `GET` | `/api/v1/stocks/{symbol}` | 股票主檔與 TWSE 即時報價 |
| `POST` | `/api/v1/orders` | 建立買進或賣出委託單 |
| `GET` | `/api/v1/orders/{id}` | 依委託單 ID 查詢委託 |
| `GET` | `/health` | 服務健康狀態，無須 API Key |

API 統一使用以下格式回傳：

```json
{
  "success": true,
  "data": {},
  "error": null,
  "traceId": "0HNDAMPLE0001"
}
```

主要 HTTP 狀態碼：

- `400 Bad Request`：Model 驗證失敗或違反業務規則。
- `401 Unauthorized`：未提供或提供錯誤的 API Key。
- `404 Not Found`：找不到股票或委託單。
- `503 Service Unavailable`：TWSE 即時報價服務無法使用或逾時。

股票明細包含可為 `null` 的 `lastPrice`、`previousClose`、`openPrice`、`highPrice`、`lowPrice`、`accumulatedVolume` 與 `quotedAt` 欄位。無成交價時，TWSE 可能回傳 `-`，API 會將該值轉換成 `null`。

## 資料來源與 InMemory 儲存

服務啟動時會從 TWSE OpenAPI `t187ap03_L` 載入上市公司代號與名稱至 thread-safe InMemory repository。這份資料是上市公司主檔，不代表完整的 ETF 或 ETN 清單。

股票主檔及委託單皆儲存在記憶體中，因此：

- 重新啟動服務時會重新載入股票主檔。
- 重新啟動服務時會清除所有委託單。
- `database/schema.sql` 是正式關聯式資料庫的結構設計，不是目前 API 實際使用的儲存來源。

## 分層架構

- `Domain`：股票、報價、委託單 Entity 與 Enum。
- `Application`：Use Case、業務規則、Service，以及 Repository／外部 Client 抽象介面。
- `Infrastructure`：InMemory Repository、API Key 驗證及 TWSE Client 實作。
- `Controllers`：處理 HTTP Request、呼叫 Application Service 並決定 HTTP Status Code。
- `Middleware`：統一回傳格式、Exception Mapping 與 Request Log。
- `Models`：API Request、Response DTO 及分頁模型。

Application 層只依賴介面，不直接依賴 InMemory 或 TWSE 實作，因此未來可替換成 EF Core Repository 或其他行情來源。

## 快取

專案使用 `IMemoryCache`：

- 股票列表查詢快取 30 秒。
- TWSE 即時報價快取 5 秒。

短效報價快取可降低對 TWSE 的重複請求，同時避免即時價格長時間停留在舊資料。

## Log 設計

專案使用 NLog 及 typed `IStructuredLogger<T>`。Console 與 rolling file 皆採用固定 JSON 結構，包含：

- Environment
- Call Site
- Service Tag
- Error Code
- Trace ID
- 經過清理的 Structured Parameters
- Exception 資訊

Log 不會記錄 API Key。完整欄位契約與使用規則請參考 `docs/logging.md`。

## 資料庫設計

正式環境使用的關聯式資料庫設計位於：

```text
database/schema.sql
```

設計內容包含 stocks 與 orders、外鍵、資料檢查條件、查詢索引、UTC 時間、價格精度及 `rowversion` 樂觀鎖定。

## 單元測試

在 solution 根目錄執行：

```powershell
dotnet test .\cathaysec_TakeHomeTest.slnx
```

測試範圍包含股票分頁、股票不存在、股票主檔查詢、即時報價快取、極端頁碼、建立委託及未知股票委託等情境。

## 壓力測試

壓力測試腳本位於：

```text
load-tests/stocks.js
```

腳本使用 k6 測試以下端點：

- `GET /api/v1/stocks?page=1&pageSize=10`
- `GET /api/v1/stocks/2330`

### 1. 安裝 k6

Windows 可使用 Windows Package Manager 安裝：

```powershell
winget install k6 --source winget
```

安裝完成後重新開啟 PowerShell，確認 k6 可以執行：

```powershell
k6 version
```

也可以使用 Chocolatey：

```powershell
choco install k6
```

### 2. 以 Release 模式啟動 API

開啟第一個 PowerShell，在 solution 根目錄執行：

```powershell
dotnet run -c Release --project .\cathaysec_TakeHomeTest\cathaysec_TakeHomeTest.csproj
```

確認終端機顯示服務正在監聽 `http://localhost:5155`。壓力測試期間請保持 API 運行。

### 3. 執行壓力測試

開啟第二個 PowerShell，在 solution 根目錄執行：

```powershell
k6 run `
  -e BASE_URL=http://localhost:5155 `
  -e API_KEY=cathaysec-dev-key `
  -e VUS=20 `
  .\load-tests\stocks.js
```

也可以使用單行命令：

```powershell
k6 run -e BASE_URL=http://localhost:5155 -e API_KEY=cathaysec-dev-key -e VUS=20 .\load-tests\stocks.js
```

### 4. 環境變數

| 變數 | 預設值 | 說明 |
|---|---|---|
| `BASE_URL` | `http://localhost:5155` | 被測試的 API Base URL |
| `API_KEY` | `cathaysec-dev-key` | 放入 `X-API-Key` Header 的金鑰 |
| `VUS` | `20` | 測試目標虛擬使用者數量 |

例如將負載提高至 50 個虛擬使用者：

```powershell
k6 run -e VUS=50 .\load-tests\stocks.js
```

未指定其他環境變數時，腳本會使用預設的 Base URL 與 API Key。

### 5. 負載階段

目前腳本共執行約 60 秒：

1. 前 15 秒逐步增加到指定的 `VUS`。
2. 接著 30 秒維持指定的 `VUS`。
3. 最後 15 秒逐步降至 0。

每個虛擬使用者會查詢一次股票列表及一次 `2330` 股票明細，完成檢查後等待 0.2 秒，再開始下一次迴圈。

### 6. 驗收門檻

腳本設定以下 threshold：

```javascript
thresholds: {
  http_req_failed: ['rate<0.01'],
  http_req_duration: ['p(95)<500'],
}
```

代表：

- HTTP Request 失敗率必須低於 1%。
- 95% 的 HTTP Request 必須在 500ms 內完成。

任一門檻未通過時，k6 會以非 0 exit code 結束，可供 CI/CD 判定壓力測試失敗。

### 7. 結果判讀

執行完成後，建議優先查看：

- `checks`：腳本中的 HTTP 200 檢查通過率。
- `http_req_failed`：HTTP Request 失敗率。
- `http_req_duration`：Request 平均、P90、P95 等回應時間。
- `http_reqs`：總 Request 數與每秒 Request 數。
- `iterations`：完成的虛擬使用者流程數。
- `vus`／`vus_max`：執行中與最高配置的虛擬使用者數。

壓力測試結果會同時受到執行電腦、網路、TWSE 回應速度、Debug／Release 模式和背景程式影響。正式量測應固定測試環境並重複執行，避免只用單次結果判斷系統容量。

### 8. 其他壓力測試情境

- Smoke Test：用少量流量快速確認腳本與服務是否正常。
- Load Test：模擬預期的一般或尖峰使用量，目前腳本屬於此類。
- Stress Test：逐步提高流量，找出效能開始明顯惡化的位置。
- Spike Test：瞬間加入大量流量，模擬開盤或重大消息造成的突發請求。
- Soak Test：長時間維持負載，檢查記憶體洩漏、資源耗盡或效能逐漸下降。
- Breakpoint Test：持續提高負載直到超過效能門檻，以找出系統容量上限。

壓力測試不應直接對未經允許的正式環境或 TWSE 服務進行高強度測試。若要測量 API 本身的最大容量，建議將外部 TWSE Client 替換成可控制的 Stub／Mock Server，避免將第三方服務延遲與流量限制混入測試結果。

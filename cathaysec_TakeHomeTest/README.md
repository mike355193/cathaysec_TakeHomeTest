# Securities Trading Data API

.NET 10 controller-based RESTful API for stock search, live TWSE quotes, and in-memory order management.

## Run

```powershell
dotnet restore .\cathaysec_TakeHomeTest.slnx
dotnet run --project .\cathaysec_TakeHomeTest\cathaysec_TakeHomeTest.csproj
```

Swagger UI: `http://localhost:5155/swagger`

All `/api/v1` endpoints require `X-API-Key: cathaysec-dev-key`. The committed key is development-only. Override `Authentication__ApiKey` with Secret Manager or a production secret store outside local development.

## Docker Compose

Visual Studio Docker Compose starts only the Web API:

- Swagger: `https://localhost:612/swagger`

At startup, the API loads listed-company symbols and names from the TWSE OpenAPI `t187ap03_L` into a thread-safe InMemory repository. This dataset contains listed companies and should not be interpreted as the complete ETF/ETN product list. Orders are also stored in memory. Restarting the process reloads stocks and clears orders. `database/schema.sql` is a separate database-structure design deliverable and is not used by the running API.

## Endpoints

- `GET /api/v1/stocks?symbol=&keyword=&page=1&pageSize=20`
- `GET /api/v1/stocks/{symbol}`
- `POST /api/v1/orders`
- `GET /api/v1/orders/{id}`
- `GET /health`

Responses use `{ success, data, error, traceId }`. Model validation and business failures return 400; missing stocks/orders return 404; missing or invalid API keys return 401.

## Design

- `Domain`: stock and order entities/enums.
- `Application`: use cases, DTO-facing services, repository/client abstractions.
- `Infrastructure`: thread-safe InMemory repositories, API-key handler, TWSE client.
- `Controllers` and `Middleware`: HTTP boundary, unified response and exception mapping.
- `IMemoryCache`: five-second quote cache to reduce TWSE traffic.
- Structured `ILogger<T>` events include order IDs, symbols, quantities, prices, error codes, and trace IDs without logging the API key.

## Logging

The application uses NLog and a typed `IStructuredLogger<T>` abstraction. Console and rolling files share a fixed JSON schema containing environment, compiler-generated call site, safe structured parameters, service tag, error code, trace ID, and exception details. See `docs/logging.md` for the complete contract and usage rules.

The production-oriented relational design is in `database/schema.sql`. It includes foreign keys, checks, search/history indexes, UTC timestamps, decimal price precision, and optimistic concurrency.

## Test

```powershell
dotnet test .\cathaysec_TakeHomeTest.slnx
```

## Load test

With the API running and [k6](https://grafana.com/docs/k6/latest/set-up/install-k6/) installed:

```powershell
k6 run -e BASE_URL=http://localhost:5155 -e API_KEY=cathaysec-dev-key -e VUS=20 .\load-tests\stocks.js
```

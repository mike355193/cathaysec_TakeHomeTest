namespace CathaySec.Api.Domain;

public sealed record StockQuote(
    decimal? LastPrice,
    decimal? PreviousClose,
    decimal? OpenPrice,
    decimal? HighPrice,
    decimal? LowPrice,
    long? AccumulatedVolume,
    DateTimeOffset? QuotedAt);

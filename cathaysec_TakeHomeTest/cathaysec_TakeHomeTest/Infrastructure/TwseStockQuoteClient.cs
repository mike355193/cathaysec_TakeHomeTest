using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CathaySec.Api.Application;
using CathaySec.Api.Domain;

namespace CathaySec.Api.Infrastructure;

public sealed class TwseStockQuoteClient(HttpClient httpClient) : IStockQuoteClient
{
    public async Task<StockQuote?> GetAsync(string symbol, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<TwseQuoteResponse>(
                $"getStockInfo.jsp?ex_ch=tse_{Uri.EscapeDataString(symbol)}.tw",
                cancellationToken);
            var quote = response?.Messages.FirstOrDefault(message =>
                message.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));

            return quote is null
                ? null
                : new StockQuote(
                    ParseDecimal(quote.LastPrice),
                    ParseDecimal(quote.PreviousClose),
                    ParseDecimal(quote.OpenPrice),
                    ParseDecimal(quote.HighPrice),
                    ParseDecimal(quote.LowPrice),
                    ParseLong(quote.AccumulatedVolume),
                    ParseTimestamp(quote.Timestamp));
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new UpstreamServiceException("TWSE real-time quote service timed out.", exception);
        }
        catch (Exception exception) when (
            exception is HttpRequestException or InvalidOperationException or JsonException)
        {
            throw new UpstreamServiceException("TWSE real-time quote service is unavailable.", exception);
        }
    }

    private static decimal? ParseDecimal(string? value) =>
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;

    private static long? ParseLong(string? value) =>
        long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;

    private static DateTimeOffset? ParseTimestamp(string? value) =>
        long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var milliseconds)
            ? DateTimeOffset.FromUnixTimeMilliseconds(milliseconds)
            : null;

    private sealed class TwseQuoteResponse
    {
        [JsonPropertyName("msgArray")]
        public TwseQuoteMessage[] Messages { get; init; } = [];
    }

    private sealed class TwseQuoteMessage
    {
        [JsonPropertyName("c")]
        public string Symbol { get; init; } = string.Empty;

        [JsonPropertyName("z")]
        public string? LastPrice { get; init; }

        [JsonPropertyName("y")]
        public string? PreviousClose { get; init; }

        [JsonPropertyName("o")]
        public string? OpenPrice { get; init; }

        [JsonPropertyName("h")]
        public string? HighPrice { get; init; }

        [JsonPropertyName("l")]
        public string? LowPrice { get; init; }

        [JsonPropertyName("v")]
        public string? AccumulatedVolume { get; init; }

        [JsonPropertyName("tlong")]
        public string? Timestamp { get; init; }
    }
}

using System.Net.Http.Json;
using System.Text.Json.Serialization;
using CathaySec.Api.Application;
using CathaySec.Api.Domain;

namespace CathaySec.Api.Infrastructure;

public sealed class TwseStockMasterDataClient(HttpClient httpClient) : IStockMasterDataClient
{
    public async Task<IReadOnlyList<Stock>> GetListedStocksAsync(CancellationToken cancellationToken)
    {
        var companies = await httpClient.GetFromJsonAsync<List<TwseListedCompany>>(
            "v1/opendata/t187ap03_L",
            cancellationToken) ?? [];

        return companies
            .Where(company => !string.IsNullOrWhiteSpace(company.Symbol))
            .Select(company => new Stock(
                company.Symbol.Trim().ToUpperInvariant(),
                FormatName(company.ShortName, company.CompanyName)))
            .DistinctBy(stock => stock.Symbol)
            .OrderBy(stock => stock.Symbol)
            .ToArray();
    }

    private static string FormatName(string? shortName, string? companyName)
    {
        var normalizedShortName = shortName?.Trim();
        var normalizedCompanyName = companyName?.Trim() ?? string.Empty;
        return string.IsNullOrWhiteSpace(normalizedShortName)
            ? normalizedCompanyName
            : $"（{normalizedShortName}）{normalizedCompanyName}";
    }

    private sealed class TwseListedCompany
    {
        [JsonPropertyName("公司代號")]
        public string Symbol { get; init; } = string.Empty;

        [JsonPropertyName("公司名稱")]
        public string CompanyName { get; init; } = string.Empty;

        [JsonPropertyName("公司簡稱")]
        public string ShortName { get; init; } = string.Empty;
    }
}

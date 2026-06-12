using System.Collections;

namespace CathaySec.Api.Logging;

internal static class LogParameterSanitizer
{
    private const int MaxCollectionItems = 20;
    private const int MaxStringLength = 500;

    private static readonly string[] SensitiveNames =
    [
        "password", "secret", "token", "authorization", "apikey", "api-key", "cookie"
    ];

    public static IReadOnlyDictionary<string, object?> Sanitize(
        IReadOnlyDictionary<string, object?>? parameters)
    {
        if (parameters is null || parameters.Count == 0)
        {
            return new Dictionary<string, object?>();
        }

        return parameters
            .Take(MaxCollectionItems)
            .ToDictionary(pair => pair.Key, pair => SanitizeValue(pair.Key, pair.Value));
    }

    private static object? SanitizeValue(string name, object? value)
    {
        if (SensitiveNames.Any(sensitive => name.Contains(sensitive, StringComparison.OrdinalIgnoreCase)) &&
            value is not bool)
        {
            return "***REDACTED***";
        }

        return value switch
        {
            null => null,
            string text => text[..Math.Min(text.Length, MaxStringLength)],
            char or bool or byte or sbyte or short or ushort or int or uint or long or ulong or
                float or double or decimal or Guid or DateTime or DateTimeOffset or TimeSpan or Enum => value,
            IEnumerable values => values.Cast<object?>().Take(MaxCollectionItems).Select(item =>
                item is null ? null : SanitizeValue("item", item)).ToArray(),
            _ => value.ToString()?[..Math.Min(value.ToString()!.Length, MaxStringLength)]
        };
    }
}

using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using CathaySec.Api.Models;
using CathaySec.Api.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace CathaySec.Api.Infrastructure.Authentication;

public sealed class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IConfiguration configuration,
    IStructuredLogger<ApiKeyAuthenticationHandler> structuredLogger)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "ApiKey";
    public const string HeaderName = "X-API-Key";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var suppliedKey))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var configuredKey = configuration["Authentication:ApiKey"];
        if (string.IsNullOrWhiteSpace(configuredKey) || suppliedKey.Count != 1 ||
            !string.Equals(suppliedKey[0], configuredKey, StringComparison.Ordinal))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));
        }

        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "api-client")], SchemeName);
        return Task.FromResult(AuthenticateResult.Success(
            new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        structuredLogger.Log(
            LogLevel.Warning,
            "API Key 驗證失敗。",
            LogTag.Authentication,
            new Dictionary<string, object?>
            {
                ["TraceId"] = Context.TraceIdentifier,
                ["Method"] = Request.Method,
                ["Path"] = Request.Path.Value,
                ["HasApiKeyHeader"] = Request.Headers.ContainsKey(HeaderName)
            },
            "UNAUTHORIZED");

        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.ContentType = "application/json";
        var body = ApiResponse<object>.Failure("UNAUTHORIZED", "A valid API key is required.")
            .WithTraceId(Context.TraceIdentifier);
        await Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
}

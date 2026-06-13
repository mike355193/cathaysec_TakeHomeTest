using System.Reflection;
using System.Runtime.CompilerServices;

namespace CathaySec.Api.Logging;

public sealed class StructuredLogger<T>(
    ILogger<T> logger,
    IHostEnvironment environment) : IStructuredLogger<T>
{
    private static readonly string ProjectName =
        Assembly.GetEntryAssembly()?.GetName().Name ?? "CathaySec.Api";

    public void Log(
        LogLevel logLevel,
        string message,
        LogTag serviceTag,
        IReadOnlyDictionary<string, object?>? parameters = null,
        string errorCode = "-",
        Exception? exception = null,
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        var callSite = BuildCallSite(sourceFilePath, memberName, sourceLineNumber);
        var traceId = parameters?.GetValueOrDefault("TraceId")?.ToString() ?? string.Empty;
        var parametersWithoutTraceId = parameters?
            .Where(pair => !pair.Key.Equals("TraceId", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(pair => pair.Key, pair => pair.Value);
        var safeParameters = LogParameterSanitizer.Sanitize(parametersWithoutTraceId);
        using var logScope = logger.BeginScope(new Dictionary<string, object>
        {
            ["TraceId"] = traceId
        });

        logger.Log(
            logLevel,
            exception,
            "{LogMessage} | {EnvironmentName} | {CallSite} | {ServiceTag} | {ErrorCode} | {@Parameters}",
            message,
            environment.EnvironmentName,
            callSite,
            serviceTag.ToString(),
            errorCode,
            safeParameters);
    }

    private static string BuildCallSite(string sourceFilePath, string memberName, int sourceLineNumber)
    {
        // CallerFilePath may contain Windows separators even when the app runs in a Linux container.
        var normalizedPath = sourceFilePath.Replace('\\', '/');
        var fileName = Path.GetFileNameWithoutExtension(normalizedPath);
        return $"{ProjectName}.{fileName}.{memberName}:{sourceLineNumber}";
    }
}

public sealed class NullStructuredLogger<T> : IStructuredLogger<T>
{
    public void Log(
        LogLevel logLevel,
        string message,
        LogTag serviceTag,
        IReadOnlyDictionary<string, object?>? parameters = null,
        string errorCode = "-",
        Exception? exception = null,
        string sourceFilePath = "",
        string memberName = "",
        int sourceLineNumber = 0)
    {
    }
}

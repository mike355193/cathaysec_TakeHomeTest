using System.Runtime.CompilerServices;

namespace CathaySec.Api.Logging;

public interface IStructuredLogger<T>
{
    void Log(
        LogLevel logLevel,
        string message,
        LogTag serviceTag,
        IReadOnlyDictionary<string, object?>? parameters = null,
        string errorCode = "-",
        Exception? exception = null,
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int sourceLineNumber = 0);
}

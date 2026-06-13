namespace CathaySec.Api.Application;

public abstract class ApplicationExceptionBase(string message) : Exception(message);

public sealed class ResourceNotFoundException(string message) : ApplicationExceptionBase(message);

public sealed class BusinessRuleException(string message) : ApplicationExceptionBase(message);

public sealed class UpstreamServiceException(string message, Exception? innerException = null)
    : Exception(message, innerException);

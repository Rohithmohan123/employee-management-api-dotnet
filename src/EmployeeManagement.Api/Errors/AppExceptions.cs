namespace EmployeeManagement.Api.Errors;

/// <summary>Business errors mapped to HTTP status codes by <see cref="GlobalExceptionHandler"/>.</summary>
public abstract class AppException(string message, int statusCode, string title) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string Title { get; } = title;
}

public sealed class NotFoundException(string entity, object key)
    : AppException($"{entity} with id '{key}' was not found.", StatusCodes.Status404NotFound, "Resource not found");

public sealed class ConflictException(string message)
    : AppException(message, StatusCodes.Status409Conflict, "Conflict");

public sealed class BusinessRuleException(string message)
    : AppException(message, StatusCodes.Status422UnprocessableEntity, "Business rule violated");

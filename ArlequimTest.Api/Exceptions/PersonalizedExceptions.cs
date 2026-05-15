namespace ArlequimTest.Api.Exceptions;

public class InternalServerError : AppException
{
    public InternalServerError(string message, int statusCode = 500) : base(message ?? "An unexpected internal error occurred", statusCode) { }
}

public class ServiceError : AppException
{
    public ServiceError(string message, int statusCode = 500) : base(message ?? "Service is currently unavailable", statusCode) { }
}

public class MethodNotAllowedError : AppException
{
    public MethodNotAllowedError(string message, int statusCode = 405) : base(message ?? "HTTP method not allowed for this endpoint", statusCode) { }
}

public class ValidationError : AppException
{
    public ValidationError(string message, int statusCode = 400) : base(message ?? "Invalid request data", statusCode) { }
}

public class NotFoundError : AppException
{
    public NotFoundError(string message, int statusCode = 404) : base(message ?? "Resource not found", statusCode) { }
}

public class UnauthorizedError : AppException
{
    public UnauthorizedError(string message, int statusCode = 401) : base(message ?? "User not authenticated", statusCode) { }
}

public class ConflictError : AppException
{
    public ConflictError(string message) : base(message ?? "Resource already exists", 409) { }
}

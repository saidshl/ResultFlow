using Common.Results;

namespace SimpleResult.Errors;

public static class PredefinedErrors
{
    public static ValidationError ValidationFailed(string message, string? details = null) =>
        new("VALIDATION_FAILED", message, details);

    public static BadRequestError BadRequest(string message, string? details = null) =>
        new("BAD_REQUEST", message, details);

    public static NotFoundError NotFound(string resourceName, string? details = null) =>
        new("NOT_FOUND", $"The requested {resourceName} was not found.", details);

    public static UnauthorizedError Unauthorized(string? details = null) =>
        new("UNAUTHORIZED", "The request requires authentication.", details);

    public static ConflictError Conflict(string message, string? details = null) =>
        new("CONFLICT", message, details);

    public static InternalServerError InternalError(string message = "An internal server error occurred.", Exception? ex = null) =>
        new("INTERNAL_SERVER_ERROR", message, ex?.Message, InnerException: ex);

    public static Error CustomError(string code, string message, string? details = null) =>
        new(code, message, details);
}

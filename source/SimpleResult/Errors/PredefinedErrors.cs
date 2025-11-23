using SimpleResult.Errors;
using SimpleResult.Errors.Constants;

public static class PredefinedErrors
{
    public static ValidationError ValidationFailed(string message, string? details = null) =>
        new(ErrorCodes.UnprocessableEntity.ValidationFailed, message, details);

    public static BadRequestError BadRequest(string message, string? details = null) =>
        new(ErrorCodes.BadRequest.Code, message, details);

    public static NotFoundError NotFound(string resourceName, string? details = null) =>
        new(ErrorCodes.NotFound.Code, $"The requested {resourceName} was not found.", details);

    public static UnauthorizedError Unauthorized(string? details = null) =>
        new(ErrorCodes.Unauthorized.Code, "The request requires authentication.", details);

    public static ConflictError Conflict(string message, string? details = null) =>
        new(ErrorCodes.Conflict.Code, message, details);

    public static InternalServerError InternalError(string message = "An internal server error occurred.", Exception? ex = null) =>
        new(ErrorCodes.InternalServer.Code, message, ex?.Message, InnerException: ex);

    public static Error CustomError(string code, string message, string? details = null) =>
        new(code, message, details);
}

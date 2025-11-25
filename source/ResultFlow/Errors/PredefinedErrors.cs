using ResultFlow.Errors;
using ResultFlow.Errors.Constants;

/// <summary>
/// Provides convenient factory methods for creating common error types with predefined error codes.
/// Use these methods for quick error creation in typical scenarios.
/// </summary>
/// <remarks>
/// This static class offers simplified error construction for standard HTTP error cases without requiring
/// manual error code lookup or complex metadata configuration. For more advanced scenarios with custom
/// metadata or specific error contexts, use the error type factory methods directly (e.g., 
/// <see cref="NotFoundError.ForResource"/>, <see cref="BadRequestError.ForInvalidParameter"/>).
/// </remarks>
public static class PredefinedErrors
{
    /// <summary>
    /// Creates a validation error indicating that input validation has failed.
    /// </summary>
    /// <param name="message">The validation failure message describing what went wrong.</param>
    /// <param name="details">Optional additional details about the validation failure, such as field-specific errors or constraints violated.</param>
    /// <returns>A <see cref="ValidationError"/> with the standard validation failed error code.</returns>
    public static ValidationError ValidationFailed(string message, string? details = null) =>
        new(ErrorCodes.UnprocessableEntity.ValidationFailed, message, details);

    /// <summary>
    /// Creates a bad request error indicating that the client request is invalid or malformed.
    /// </summary>
    /// <param name="message">The error message describing why the request is invalid.</param>
    /// <param name="details">Optional additional details about the bad request, such as which parameters are problematic.</param>
    /// <returns>A <see cref="BadRequestError"/> with the standard bad request error code.</returns>
    public static BadRequestError BadRequest(string message, string? details = null) =>
        new(ErrorCodes.BadRequest.Code, message, details);

    /// <summary>
    /// Creates a not found error indicating that the requested resource could not be located.
    /// </summary>
    /// <param name="resourceName">The name of the resource type that was not found (e.g., "User", "Product", "Order").</param>
    /// <param name="details">Optional additional details about the missing resource, such as search criteria used.</param>
    /// <returns>A <see cref="NotFoundError"/> with a message stating the resource was not found.</returns>
    public static NotFoundError NotFound(string resourceName, string? details = null) =>
        new(ErrorCodes.NotFound.Code, $"The requested {resourceName} was not found.", details);

    /// <summary>
    /// Creates an unauthorized error indicating that the request lacks valid authentication credentials.
    /// </summary>
    /// <param name="details">Optional additional details about the authentication failure, such as token expiration or missing headers.</param>
    /// <returns>A <see cref="UnauthorizedError"/> with the standard authentication required message.</returns>
    public static UnauthorizedError Unauthorized(string? details = null) =>
        new(ErrorCodes.Unauthorized.Code, "The request requires authentication.", details);

    /// <summary>
    /// Creates a new conflict error indicating that a request could not be completed due to a resource conflict.
    /// </summary>
    /// <param name="message">The error message describing the nature of the conflict. Cannot be null.</param>
    /// <param name="details">Optional additional details providing context about the conflict. May be null if no further information is
    /// available.</param>
    /// <returns>A <see cref="ConflictError"/> representing the conflict error, containing the specified message and details.</returns>
    public static ConflictError Conflict(string message, string? details = null) =>
        new(ErrorCodes.Conflict.Code, message, details);

    /// <summary>
    /// Creates a new instance of the <see cref="InternalServerError"/> class representing an internal server error.
    /// </summary>
    /// <param name="message">The error message to associate with the internal server error. If not specified, a default message is used.</param>
    /// <param name="ex">An optional exception that provides additional context for the error. If specified, its message is included in
    /// the error details.</param>
    /// <returns>An <see cref="InternalServerError"/> object containing the specified error message and exception details.</returns>
    public static InternalServerError InternalError(string message = "An internal server error occurred.", Exception? ex = null) =>
        new(ErrorCodes.InternalServer.Code, message, ex?.Message, InnerException: ex);

    /// <summary>
    /// Creates a new <see cref="Error"/> instance with the specified error code, message, and optional details.
    /// </summary>
    /// <param name="code">The error code that identifies the type or category of the error. Cannot be null.</param>
    /// <param name="message">The human-readable message describing the error. Cannot be null.</param>
    /// <param name="details">Optional additional information about the error. Can be <see langword="null"/> if no extra details are
    /// available.</param>
    /// <returns>An <see cref="Error"/> object containing the provided code, message, and details.</returns>
    public static Error CustomError(string code, string message, string? details = null) =>
        new(code, message, details);
}

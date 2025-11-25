using EasyResult.Errors.Constants;

namespace EasyResult.Errors;

/// <summary>
/// Represents an error indicating that an unexpected internal server condition has occurred. Used to encapsulate
/// details about server-side failures that are not exposed to the client.
/// </summary>
/// <remarks>Use this type to standardize error handling for unexpected server failures. The metadata and details
/// properties can be leveraged for diagnostics and logging. This error type is not intended for client-visible
/// validation or business logic errors.</remarks>
/// <param name="Code">The error code identifying the internal server error. Typically set to "INTERNAL_SERVER_ERROR" or a custom code for
/// specific scenarios.</param>
/// <param name="Message">The error message describing the nature of the internal server error.</param>
/// <param name="Details">Optional additional details providing further context about the error. Can include stack traces or diagnostic
/// information.</param>
/// <param name="Metadata">Optional metadata dictionary containing contextual information related to the error, such as operation names or
/// exception types.</param>
/// <param name="InnerException">The underlying exception that caused the internal server error, if available.</param>
public record InternalServerError(
    string Code,
    string Message,
    string? Details = null,
    Dictionary<string, object>? Metadata = null,
    Exception? InnerException = null
) : Error(Code, Message, Details, Metadata, InnerException)
{
    /// <summary>
    /// Creates an internal server error with the standard internal server error code.
    /// </summary>
    public static InternalServerError WithDefaults(
        string message = "An internal server error occurred.",
        string? details = null,
        Dictionary<string, object>? metadata = null) =>
        new(ErrorCodes.InternalServer.Code, message, details, metadata);

    /// <summary>
    /// Creates an internal server error from an exception.
    /// </summary>
    public static InternalServerError FromException(
        Exception exception,
        string? message = null,
        string? details = null) =>
        new(ErrorCodes.InternalServer.Code,
            message ?? exception.Message ?? "An internal server error occurred.",
            details ?? exception.StackTrace,
            new Dictionary<string, object>
            {
                { "exceptionType", exception.GetType().FullName ?? "Unknown" },
                { "exceptionMessage", exception.Message ?? string.Empty }
            },
            exception);

    /// <summary>
    /// Creates an internal server error with a specific operation context.
    /// </summary>
    public static InternalServerError ForOperation(
        string operation,
        Exception exception,
        string? details = null) =>
        new(ErrorCodes.InternalServer.Code,
            $"An error occurred while processing the {operation} operation.",
            details ?? exception.StackTrace,
            new Dictionary<string, object>
            {
                { "operation", operation },
                { "exceptionType", exception.GetType().FullName ?? "Unknown" },
                { "exceptionMessage", exception.Message ?? string.Empty }
            },
            exception);

    /// <summary>
    /// Creates an internal server error with a custom error code and full context.
    /// </summary>
    public static InternalServerError WithCode(
        string errorCode,
        Exception exception,
        string? message = null,
        string? details = null) =>
        new(errorCode,
            message ?? exception.Message ?? "An internal server error occurred.",
            details ?? exception.StackTrace,
            new Dictionary<string, object>
            {
                { "exceptionType", exception.GetType().FullName ?? "Unknown" },
                { "exceptionMessage", exception.Message ?? string.Empty }
            },
            exception);
}

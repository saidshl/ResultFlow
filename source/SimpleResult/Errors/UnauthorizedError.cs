using SimpleResult.Errors.Constants;

namespace SimpleResult.Errors;

/// <summary>
/// Represents an error indicating that a request was denied due to missing or invalid authentication credentials.
/// </summary>
/// <remarks>Use this type to signal authentication failures in APIs or services. The error includes a standard
/// code and message, with optional details and metadata for further context. This record is typically returned when a
/// request lacks valid authentication or authorization.</remarks>
/// <param name="Code">The error code identifying the unauthorized error. Typically set to "UNAUTHORIZED".</param>
/// <param name="Message">The error message describing the unauthorized condition.</param>
/// <param name="Details">Optional additional details providing context about the unauthorized error.</param>
/// <param name="Metadata">Optional metadata dictionary containing supplementary information related to the error.</param>
/// <param name="InnerException">The inner exception that caused this unauthorized error, if available.</param>
public record UnauthorizedError(
    string Code,
    string Message,
    string? Details = null,
    Dictionary<string, object>? Metadata = null,
    Exception? InnerException = null
) : Error(Code, Message, Details, Metadata, InnerException)
{
    /// <summary>
    /// Creates an unauthorized error with the standard unauthorized code.
    /// </summary>
    public static UnauthorizedError WithDefaults(
        string message = "The request requires authentication.",
        string? details = null,
        Dictionary<string, object>? metadata = null) =>
        new(ErrorCodes.Unauthorized.Code, message, details, metadata);

    /// <summary>
    /// Creates an unauthorized error with a specific reason.
    /// </summary>
    public static UnauthorizedError ForReason(
        string reason,
        string? details = null) =>
        new(ErrorCodes.Unauthorized.Code,
            $"The request requires authentication. Reason: {reason}", details,
            new Dictionary<string, object> { { "reason", reason } });

    /// <summary>
    /// Creates an unauthorized error with an inner exception for debugging.
    /// </summary>
    public static UnauthorizedError WithException(
        Exception exception,
        string message = "The request requires authentication.",
        string? details = null) =>
        new(ErrorCodes.Unauthorized.Code, message, details, null, exception);
}

using ResultFlow.Errors.Constants;

namespace ResultFlow.Errors;

/// <summary>
/// Represents an error indicating that a requested resource was not found. Provides standardized error information for
/// not found scenarios, including optional details, metadata, and an inner exception for debugging purposes.
/// </summary>
/// <remarks>Use NotFoundError to represent situations where a specific resource, such as a user or product,
/// cannot be located. The metadata dictionary can be used to supply additional context, such as the resource name or
/// identifier, to aid in error handling and diagnostics.</remarks>
/// <param name="Code">The error code that identifies the type of error. Typically set to "NOT_FOUND" for not found errors.</param>
/// <param name="Message">The error message describing the not found condition.</param>
/// <param name="Details">Optional additional details that provide more context about the error.</param>
/// <param name="Metadata">Optional metadata dictionary containing contextual information related to the error, such as resource identifiers or
/// names.</param>
/// <param name="InnerException">Optional inner exception that caused the not found error, useful for debugging and error tracing.</param>
public sealed record NotFoundError(
    string Code,
    string Message,
    string? Details = null,
    Dictionary<string, object>? Metadata = null,
    Exception? InnerException = null
) : Error(Code, Message, Details, Metadata, InnerException)
{
    /// <summary>
    /// Creates a not found error with the standard not found code.
    /// </summary>
    public static NotFoundError WithDefaults(
        string message = "The requested resource was not found.",
        string? details = null,
        Dictionary<string, object>? metadata = null) =>
        new(ErrorCodes.NotFound.Code, message, details, metadata);

    /// <summary>
    /// Creates a not found error for a specific resource type.
    /// </summary>
    public static NotFoundError ForResource(
        string resourceName,
        string? resourceId = null,
        string? details = null) =>
        new(ErrorCodes.NotFound.ResourceNotFound,
            $"The requested {resourceName} was not found.", details,
            new Dictionary<string, object>
            {
                { "resourceName", resourceName },
                { "resourceId", resourceId ?? string.Empty }
            });

    /// <summary>
    /// Creates a not found error with a specific identifier.
    /// </summary>
    public static NotFoundError ByIdentifier(
        string resourceName,
        object identifier,
        string? details = null) =>
        new(ErrorCodes.NotFound.ByIdentifier,
            $"The {resourceName} with identifier '{identifier}' was not found.", details,
            new Dictionary<string, object>
            {
                { "resourceName", resourceName },
                { "identifier", identifier }
            });

    /// <summary>
    /// Creates a not found error with an inner exception for debugging.
    /// </summary>
    public static NotFoundError WithException(
        Exception exception,
        string message = "The requested resource was not found.",
        string? details = null) =>
        new(ErrorCodes.NotFound.Code, message, details, null, exception);
}

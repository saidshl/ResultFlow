using EasyResult.Errors.Constants;

namespace EasyResult.Errors;

/// <summary>
/// Represents an error indicating that a request is invalid or malformed, typically corresponding to an HTTP 400 Bad
/// Request. Provides standardized error information for scenarios where client input fails validation or does not meet
/// required criteria.
/// </summary>
/// <remarks>Use BadRequestError to represent client-side errors where the request cannot be processed due to
/// invalid input, missing required fields, or incorrect formatting. This type provides factory methods for common bad
/// request scenarios, allowing for consistent error handling and reporting. The Metadata property may include
/// additional context such as parameter names, reasons for failure, or expected formats.</remarks>
/// <param name="Code">The error code that identifies the type of bad request. Typically set to "BAD_REQUEST".</param>
/// <param name="Message">The error message describing the reason for the bad request.</param>
/// <param name="Details">Optional additional details that provide further context about the error.</param>
/// <param name="Metadata">Optional metadata dictionary containing contextual information related to the error, such as invalid parameters or
/// missing fields.</param>
/// <param name="InnerException">Optional inner exception that caused the bad request error, useful for debugging or error tracing.</param>
public record BadRequestError(
    string Code,
    string Message,
    string? Details = null,
    Dictionary<string, object>? Metadata = null,
    Exception? InnerException = null
) : Error(Code, Message, Details, Metadata, InnerException)
{
    /// <summary>
    /// Creates a bad request error with the standard bad request code.
    /// </summary>
    public static BadRequestError WithDefaults(
        string message = "The request is invalid or malformed.",
        string? details = null,
        Dictionary<string, object>? metadata = null) =>
        new(ErrorCodes.BadRequest.Code, message, details, metadata);

    /// <summary>
    /// Creates a bad request error for invalid request parameters.
    /// </summary>
    public static BadRequestError ForInvalidParameter(
        string parameterName,
        string reason,
        object? providedValue = null,
        string? details = null) =>
        new(ErrorCodes.BadRequest.InvalidParameter,
            $"The parameter '{parameterName}' is invalid: {reason}",
            details,
            new Dictionary<string, object>
            {
                { "parameterName", parameterName },
                { "reason", reason },
                { "providedValue", providedValue ?? "null" }
            });

    /// <summary>
    /// Creates a bad request error for missing required fields.
    /// </summary>
    public static BadRequestError ForMissingFields(
        IEnumerable<string> fieldNames,
        string? details = null)
    {
        var missingFields = fieldNames.ToList();
        return new(ErrorCodes.BadRequest.MissingFields,
            $"The following required fields are missing: {string.Join(", ", missingFields)}",
            details,
            new Dictionary<string, object>
            {
                { "missingFields", missingFields },
                { "count", missingFields.Count }
            });
    }

    /// <summary>
    /// Creates a bad request error for a single missing required field.
    /// </summary>
    public static BadRequestError ForMissingField(
        string fieldName,
        string? details = null) =>
        new(ErrorCodes.BadRequest.MissingFields,
            $"The required field '{fieldName}' is missing.",
            details,
            new Dictionary<string, object> { { "fieldName", fieldName } });

    /// <summary>
    /// Creates a bad request error for invalid request format.
    /// </summary>
    public static BadRequestError ForInvalidFormat(
        string formatType,
        string? details = null) =>
        new(ErrorCodes.BadRequest.InvalidFormat,
            $"The request format is invalid. Expected format: {formatType}",
            details,
            new Dictionary<string, object> { { "expectedFormat", formatType } });

    /// <summary>
    /// Creates a bad request error with an inner exception for debugging.
    /// </summary>
    public static BadRequestError WithException(
        Exception exception,
        string message = "The request is invalid or malformed.",
        string? details = null) =>
        new(ErrorCodes.BadRequest.Code, message, details, null, exception);
}

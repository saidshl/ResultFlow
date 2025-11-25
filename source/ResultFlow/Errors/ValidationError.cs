using ResultFlow.Errors.Constants;

namespace ResultFlow.Errors;

/// <summary>
/// Represents an error that occurs when input data fails to meet validation requirements.
/// </summary>
/// <remarks>Use this type to represent errors resulting from invalid or missing input data, such as failed field
/// validation or business rule violations. The standard validation error code is "VALIDATION_ERROR". Additional
/// context, such as the affected field, can be provided in the metadata dictionary.</remarks>
/// <param name="Code">The error code that identifies the type of validation error.</param>
/// <param name="Message">The message describing the validation failure.</param>
/// <param name="Details">Optional. Additional details about the validation error, or null if not specified.</param>
/// <param name="Metadata">Optional. A dictionary containing supplementary metadata related to the error, or null if not specified.</param>
/// <param name="InnerException">Optional. The underlying exception that caused this validation error, or null if not applicable.</param>
public sealed record ValidationError(
    string Code,
    string Message,
    string? Details = null,
    Dictionary<string, object>? Metadata = null,
    Exception? InnerException = null
) : Error(Code, Message, Details, Metadata, InnerException)
{
    /// <summary>
    /// Creates a validation error with the standard validation code.
    /// </summary>
    public static ValidationError WithDefaults(
        string message,
        string? details = null,
        Dictionary<string, object>? metadata = null) =>
        new(ErrorCodes.UnprocessableEntity.ValidationFailed, message, details, metadata);

    /// <summary>
    /// Creates a validation error for a specific field.
    /// </summary>
    public static ValidationError ForField(
        string fieldName,
        string message,
        string? details = null) =>
        new(ErrorCodes.UnprocessableEntity.ValidationFailed, message, details,
            new Dictionary<string, object> { { "field", fieldName } });
}


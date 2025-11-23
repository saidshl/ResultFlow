namespace SimpleResult.Errors;

/// <summary>
/// Represents an error with a code, message, optional details, metadata, and an optional inner exception for diagnostic
/// purposes.
/// </summary>
/// <remarks>This record is commonly used to encapsulate error information for logging, diagnostics, or API
/// responses. The metadata dictionary can be used to include custom fields relevant to the error context.</remarks>
/// <param name="Code">The error code that uniquely identifies the type or category of the error. This value should be non-empty and is
/// intended for programmatic handling.</param>
/// <param name="Message">The human-readable message describing the error. This message is intended to provide context for users or
/// developers.</param>
/// <param name="Details">Optional additional information that further describes the error. Can be null if no extra details are available.</param>
/// <param name="Metadata">Optional key-value pairs containing structured metadata related to the error. Can be null if no metadata is
/// provided.</param>
/// <param name="InnerException">The underlying exception that caused this error, if available. Can be null if there is no inner exception.</param>
public record Error(
    string Code,
    string Message,
    string? Details = null,
    Dictionary<string, object>? Metadata = null,
    Exception? InnerException = null
);


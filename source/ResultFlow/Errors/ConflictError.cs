using ResultFlow.Errors.Constants;

namespace ResultFlow.Errors;

/// <summary>
/// Represents an error indicating that a request could not be completed due to a conflict with the current state of the
/// resource.
/// </summary>
/// <remarks>Use this type to signal HTTP 409 Conflict scenarios, such as duplicate resources, version mismatches,
/// or invalid resource states. Provides factory methods for common conflict cases and supports attaching metadata and
/// inner exceptions for enhanced error reporting.</remarks>
/// <param name="Code">The error code that identifies the type of conflict. Typically set to "CONFLICT".</param>
/// <param name="Message">The error message describing the nature of the conflict.</param>
/// <param name="Details">Optional additional details that provide more context about the conflict.</param>
/// <param name="Metadata">Optional metadata dictionary containing contextual information related to the conflict.</param>
/// <param name="InnerException">Optional inner exception that caused this conflict error, used for debugging or error tracing.</param>
public record ConflictError(
    string Code,
    string Message,
    string? Details = null,
    Dictionary<string, object>? Metadata = null,
    Exception? InnerException = null
) : Error(Code, Message, Details, Metadata, InnerException)
{
    /// <summary>
    /// Creates a conflict error with the standard conflict code.
    /// </summary>
    public static ConflictError WithDefaults(
        string message = "The request conflicts with the current state of the resource.",
        string? details = null,
        Dictionary<string, object>? metadata = null) =>
        new(ErrorCodes.Conflict.Code, message, details, metadata);

    /// <summary>
    /// Creates a conflict error for a duplicate resource.
    /// </summary>
    public static ConflictError ForDuplicateResource(
        string resourceName,
        string conflictingValue,
        string? details = null) =>
        new(ErrorCodes.Conflict.DuplicateResource,
            $"A {resourceName} with the value '{conflictingValue}' already exists.",
            details,
            new Dictionary<string, object>
            {
                { "resourceName", resourceName },
                { "conflictingValue", conflictingValue }
            });

    /// <summary>
    /// Creates a conflict error for a resource version mismatch.
    /// </summary>
    public static ConflictError ForVersionMismatch(
        string resourceName,
        string expectedVersion,
        string currentVersion,
        string? details = null) =>
        new(ErrorCodes.Conflict.VersionMismatch,
            $"The {resourceName} has been modified. Expected version: {expectedVersion}, Current version: {currentVersion}.",
            details,
            new Dictionary<string, object>
            {
                { "resourceName", resourceName },
                { "expectedVersion", expectedVersion },
                { "currentVersion", currentVersion }
            });

    /// <summary>
    /// Creates a conflict error for a state conflict.
    /// </summary>
    public static ConflictError ForStateConflict(
        string resourceName,
        string currentState,
        string requiredState,
        string? details = null) =>
        new(ErrorCodes.Conflict.InvalidState,
            $"Cannot perform this operation on {resourceName} in '{currentState}' state. Required state: '{requiredState}'.",
            details,
            new Dictionary<string, object>
            {
                { "resourceName", resourceName },
                { "currentState", currentState },
                { "requiredState", requiredState }
            });

    /// <summary>
    /// Creates a conflict error with an inner exception for debugging.
    /// </summary>
    public static ConflictError WithException(
        Exception exception,
        string message = "The request conflicts with the current state of the resource.",
        string? details = null) =>
        new(ErrorCodes.Conflict.Code, message, details, null, exception);
}

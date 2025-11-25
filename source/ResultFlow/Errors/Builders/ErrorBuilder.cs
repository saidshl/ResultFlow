using System.Diagnostics.CodeAnalysis;

namespace ResultFlow.Errors.Builders;

/// <summary>
/// Provides a fluent builder for constructing Error instances with customizable code, message, details, metadata, and
/// inner exception information.
/// </summary>
/// <remarks>Use ErrorBuilder to incrementally configure error information before creating an Error object. The
/// builder enforces that both error code and message are set before building an Error. Additional metadata and details
/// can be attached to enrich error context. This class is not thread-safe; concurrent modifications should be
/// synchronized externally if used across threads.</remarks>
public class ErrorBuilder
{
    private string _code = string.Empty;
    private string _message = string.Empty;
    private string? _details;
    private Dictionary<string, object>? _metadata;
    private Exception? _innerException;

    /// <summary>
    /// Creates a new ErrorBuilder instance.
    /// </summary>
    public ErrorBuilder()
    {
    }

    /// <summary>
    /// Sets the error code (required).
    /// </summary>
    /// <param name="code">The error code identifier.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ErrorBuilder WithCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Error code cannot be null or whitespace.", nameof(code));

        _code = code;
        return this;
    }

    /// <summary>
    /// Sets the error message (required).
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ErrorBuilder WithMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Error message cannot be null or whitespace.", nameof(message));

        _message = message;
        return this;
    }

    /// <summary>
    /// Sets additional details about the error.
    /// </summary>
    /// <param name="details">Detailed information about the error.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ErrorBuilder WithDetails(string details)
    {
        _details = details;
        return this;
    }

    /// <summary>
    /// Sets the complete metadata dictionary, replacing any existing metadata.
    /// </summary>
    /// <param name="metadata">The metadata dictionary.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ErrorBuilder WithMetadata(Dictionary<string, object> metadata)
    {
        _metadata = metadata ?? new Dictionary<string, object>();
        return this;
    }

    /// <summary>
    /// Adds a single metadata entry. Creates metadata dictionary if it doesn't exist.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ErrorBuilder AddMetadata(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be null or whitespace.", nameof(key));

        _metadata ??= new Dictionary<string, object>();
        _metadata[key] = value;
        return this;
    }

    /// <summary>
    /// Adds multiple metadata entries from a dictionary.
    /// </summary>
    /// <param name="metadata">Dictionary of metadata entries to add.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ErrorBuilder AddMetadataRange(Dictionary<string, object> metadata)
    {
        if (metadata == null || metadata.Count == 0)
            return this;

        _metadata ??= new Dictionary<string, object>();
        foreach (var kvp in metadata)
        {
            _metadata[kvp.Key] = kvp.Value;
        }
        return this;
    }

    /// <summary>
    /// Sets the inner exception for debugging purposes.
    /// </summary>
    /// <param name="exception">The exception that caused this error.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ErrorBuilder WithException(Exception exception)
    {
        _innerException = exception;
        if (exception != null)
        {
            AddMetadata("exceptionType", exception.GetType().FullName ?? "Unknown");
            AddMetadata("exceptionMessage", exception.Message ?? string.Empty);
        }
        return this;
    }

    /// <summary>
    /// Clears all metadata entries.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    public ErrorBuilder ClearMetadata()
    {
        _metadata?.Clear();
        return this;
    }

    /// <summary>
    /// Builds and returns the Error instance.
    /// </summary>
    /// <returns>A new Error instance with the configured values.</returns>
    /// <exception cref="InvalidOperationException">Thrown if required fields (Code or Message) are not set.</exception>
    public Error Build()
    {
        ValidateRequiredFields();
        return new Error(_code, _message, _details, _metadata, _innerException);
    }

    /// <summary>
    /// Builds and returns a typed Error instance (e.g., ValidationError, NotFoundError).
    /// </summary>
    /// <typeparam name="TError">The error type to create. Must have a public constructor with parameters (string, string, string?, Dictionary&lt;string, object&gt;?, Exception?).</typeparam>
    /// <returns>A new error instance of the specified type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if required fields are missing or if the type cannot be instantiated.</exception>
    public TError Build<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TError>()
        where TError : Error
    {
        ValidateRequiredFields();
        return (TError)Activator.CreateInstance(
            typeof(TError),
            _code,
            _message,
            _details,
            _metadata,
            _innerException)!;
    }

    /// <summary>
    /// Validates that all required fields are set.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if required fields are missing.</exception>
    private void ValidateRequiredFields()
    {
        if (string.IsNullOrWhiteSpace(_code))
            throw new InvalidOperationException("Error code is required. Use WithCode() method to set it.");

        if (string.IsNullOrWhiteSpace(_message))
            throw new InvalidOperationException("Error message is required. Use WithMessage() method to set it.");
    }

    /// <summary>
    /// Creates a new ErrorBuilder with a given code and message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A new ErrorBuilder instance.</returns>
    public static ErrorBuilder Create(string code, string message) =>
        new ErrorBuilder()
            .WithCode(code)
            .WithMessage(message);

    /// <summary>
    /// Creates a new empty ErrorBuilder.
    /// </summary>
    /// <returns>A new ErrorBuilder instance.</returns>
    public static ErrorBuilder Empty() => new();
}

using ResultFlow.Errors.Constants;

namespace ResultFlow.Errors;

/// <summary>
/// Represents a 429 Too Many Requests domain error indicating the caller has exceeded
/// the allowed rate limit or quota for the requested operation.
/// </summary>
/// <param name="Code">/// Machine-readable error code (typically one of <see cref="ErrorCodes.TooManyRequests"/> constants)./// </param>
/// <param name="Message">/// Human-readable description of the rate limit violation suitable for clients./// </param>
/// <param name="Details">/// Optional extended detail (e.g. quota policy explanation or retry guidance)./// </param>
/// <param name="Metadata">/// Optional structured key/value data providing additional context (e.g. limit, remaining, retryAfterSeconds, resetAt)./// </param>
/// <param name="InnerException">/// Optional underlying exception captured for diagnostics (not usually exposed externally)./// </param>
public sealed record TooManyRequestsError(
    string Code,
    string Message,
    string? Details = null,
    Dictionary<string, object>? Metadata = null,
    Exception? InnerException = null
) : Error(Code, Message, Details, Metadata, InnerException)
{
    /// <summary>
    /// Creates a too many requests error with the standard code.
    /// </summary>
    public static TooManyRequestsError WithDefaults(
        string message = "You have sent too many requests in a given amount of time.",
        string? details = null,
        Dictionary<string, object>? metadata = null) =>
        new(ErrorCodes.TooManyRequests.Code, message, details, metadata);

    /// <summary>
    /// Creates a too many requests error with retry information.
    /// </summary>
    public static TooManyRequestsError WithRetryAfter(
        int retryAfterSeconds,
        string? details = null) =>
        new(ErrorCodes.TooManyRequests.Code,
            $"Rate limit exceeded. Please retry after {retryAfterSeconds} seconds.",
            details,
            new Dictionary<string, object>
            {
                { "retryAfterSeconds", retryAfterSeconds },
                { "retryAfter", DateTime.UtcNow.AddSeconds(retryAfterSeconds) }
            });

    /// <summary>
    /// Creates a too many requests error with limit details.
    /// </summary>
    public static TooManyRequestsError ForRateLimit(
        int limit,
        int remaining,
        int resetAfterSeconds,
        string? details = null) =>
        new(ErrorCodes.TooManyRequests.Code,
            $"Rate limit exceeded. Limit: {limit}, Remaining: {remaining}.",
            details,
            new Dictionary<string, object>
            {
                { "limit", limit },
                { "remaining", remaining },
                { "resetAfterSeconds", resetAfterSeconds },
                { "resetAt", DateTime.UtcNow.AddSeconds(resetAfterSeconds) }
            });
}

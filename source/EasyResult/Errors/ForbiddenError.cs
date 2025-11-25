using EasyResult.Errors.Constants;

namespace EasyResult.Errors;

/// <summary>
/// Represents a 403 Forbidden domain error indicating the caller lacks required
/// authorization (role / permission) to perform the requested operation.
/// </summary>
/// <param name="Code">/// Machine-readable error code (typically one of <see cref="ErrorCodes.Forbidden"/> constants)./// </param>
/// <param name="Message">/// Human-readable description of the authorization failure suitable for clients./// </param>
/// <param name="Details">/// Optional extended detail (e.g. contextual explanation or troubleshooting guidance)./// </param>
/// <param name="Metadata">/// Optional structured key/value data providing additional context (e.g. requiredRole, userRole)./// </param>
/// <param name="InnerException">/// Optional underlying exception captured for diagnostics (not usually exposed externally)./// </param>
public record ForbiddenError(
    string Code,
    string Message,
    string? Details = null,
    Dictionary<string, object>? Metadata = null,
    Exception? InnerException = null
) : Error(Code, Message, Details, Metadata, InnerException)
{
    /// <summary>
    /// Creates a forbidden error with the standard forbidden code.
    /// </summary>
    public static ForbiddenError WithDefaults(
        string message = "You do not have permission to access this resource.",
        string? details = null,
        Dictionary<string, object>? metadata = null) =>
        new(ErrorCodes.Forbidden.Code, message, details, metadata);

    /// <summary>
    /// Creates a forbidden error for missing role.
    /// </summary>
    public static ForbiddenError ForMissingRole(
        string requiredRole,
        string? userRole = null,
        string? details = null) =>
        new(ErrorCodes.Forbidden.MissingRole,
            $"This operation requires the '{requiredRole}' role.",
            details,
            new Dictionary<string, object>
            {
                { "requiredRole", requiredRole },
                { "userRole", userRole ?? "None" }
            });

    /// <summary>
    /// Creates a forbidden error for missing permission.
    /// </summary>
    public static ForbiddenError ForMissingPermission(
        string requiredPermission,
        string? details = null) =>
        new(ErrorCodes.Forbidden.MissingPermission,
            $"This operation requires the '{requiredPermission}' permission.",
            details,
            new Dictionary<string, object>
            {
                { "requiredPermission", requiredPermission }
            });

    /// <summary>
    /// Creates a forbidden error with an inner exception for debugging.
    /// </summary>
    public static ForbiddenError WithException(
        Exception exception,
        string message = "You do not have permission to access this resource.",
        string? details = null) =>
        new(ErrorCodes.Forbidden.Code, message, details, null, exception);
}

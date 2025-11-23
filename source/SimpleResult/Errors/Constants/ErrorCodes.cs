namespace SimpleResult.Errors.Constants;


/// <summary>
/// Centralized catalog of standardized string error code constants, grouped by HTTP status
/// families and custom application domains.
/// </summary>
/// <remarks>
/// Use these constants to produce consistent, machine-readable error identifiers in API
/// responses, logging and validation results.
/// </remarks>
public static class ErrorCodes
{
    /// <summary>
    /// HTTP 400 - Bad Request error codes.
    /// </summary>
    public static class BadRequest
    {
        /// <summary>The request is invalid or malformed.</summary>
        public const string Code = "BAD_REQUEST";

        /// <summary>A parameter in the request is invalid.</summary>
        public const string InvalidParameter = "BAD_REQUEST_INVALID_PARAMETER";

        /// <summary>Required fields are missing from the request.</summary>
        public const string MissingFields = "BAD_REQUEST_MISSING_FIELDS";

        /// <summary>The request format is invalid (e.g., invalid JSON).</summary>
        public const string InvalidFormat = "BAD_REQUEST_INVALID_FORMAT";
    }

    /// <summary>
    /// HTTP 401 - Unauthorized error codes.
    /// </summary>
    public static class Unauthorized
    {
        /// <summary>The request requires authentication.</summary>
        public const string Code = "UNAUTHORIZED";

        /// <summary>Authentication credentials were not provided.</summary>
        public const string MissingCredentials = "UNAUTHORIZED_MISSING_CREDENTIALS";

        /// <summary>The provided credentials are invalid.</summary>
        public const string InvalidCredentials = "UNAUTHORIZED_INVALID_CREDENTIALS";

        /// <summary>The authentication token has expired.</summary>
        public const string TokenExpired = "UNAUTHORIZED_TOKEN_EXPIRED";

        /// <summary>The authentication token is invalid.</summary>
        public const string InvalidToken = "UNAUTHORIZED_INVALID_TOKEN";
    }

    /// <summary>
    /// HTTP 403 - Forbidden error codes.
    /// </summary>
    public static class Forbidden
    {
        /// <summary>The user does not have permission to access this resource.</summary>
        public const string Code = "FORBIDDEN";

        /// <summary>Required role is missing.</summary>
        public const string MissingRole = "FORBIDDEN_MISSING_ROLE";

        /// <summary>Required permission is missing.</summary>
        public const string MissingPermission = "FORBIDDEN_MISSING_PERMISSION";
    }

    /// <summary>
    /// HTTP 404 - Not Found error codes.
    /// </summary>
    public static class NotFound
    {
        /// <summary>The requested resource was not found.</summary>
        public const string Code = "NOT_FOUND";

        /// <summary>A specific resource type was not found.</summary>
        public const string ResourceNotFound = "NOT_FOUND_RESOURCE";

        /// <summary>A resource with a specific identifier was not found.</summary>
        public const string ByIdentifier = "NOT_FOUND_BY_IDENTIFIER";
    }

    /// <summary>
    /// HTTP 409 - Conflict error codes.
    /// </summary>
    public static class Conflict
    {
        /// <summary>The request conflicts with the current state of the resource.</summary>
        public const string Code = "CONFLICT";

        /// <summary>A duplicate resource already exists.</summary>
        public const string DuplicateResource = "CONFLICT_DUPLICATE_RESOURCE";

        /// <summary>The resource version has changed (optimistic concurrency).</summary>
        public const string VersionMismatch = "CONFLICT_VERSION_MISMATCH";

        /// <summary>The resource is in an invalid state for the requested operation.</summary>
        public const string InvalidState = "CONFLICT_INVALID_STATE";
    }

    /// <summary>
    /// HTTP 422 - Unprocessable Entity error codes.
    /// </summary>
    public static class UnprocessableEntity
    {
        /// <summary>The request entity is invalid or cannot be processed.</summary>
        public const string Code = "UNPROCESSABLE_ENTITY";

        /// <summary>Validation rules have been violated.</summary>
        public const string ValidationFailed = "UNPROCESSABLE_ENTITY_VALIDATION";
    }

    /// <summary>
    /// HTTP 500 - Internal Server Error codes.
    /// </summary>
    public static class InternalServer
    {
        /// <summary>An internal server error occurred.</summary>
        public const string Code = "INTERNAL_SERVER_ERROR";

        /// <summary>A database operation failed.</summary>
        public const string DatabaseError = "INTERNAL_SERVER_DATABASE_ERROR";

        /// <summary>An external service call failed.</summary>
        public const string ExternalServiceError = "INTERNAL_SERVER_EXTERNAL_SERVICE_ERROR";

        /// <summary>A timeout occurred during processing.</summary>
        public const string Timeout = "INTERNAL_SERVER_TIMEOUT";

        /// <summary>An unexpected error occurred.</summary>
        public const string UnexpectedError = "INTERNAL_SERVER_UNEXPECTED_ERROR";
    }

    /// <summary>
    /// HTTP 429 - Too Many Requests error codes.
    /// </summary>
    public static class TooManyRequests
    {
        /// <summary>Rate limit has been exceeded.</summary>
        public const string Code = "TOO_MANY_REQUESTS";

        /// <summary>Rate limit exceeded with retry information.</summary>
        public const string RateLimitExceeded = "TOO_MANY_REQUESTS_RATE_LIMIT";
    }

    /// <summary>
    /// Custom application-specific error codes.
    /// </summary>
    public static class Custom
    {
        /// <summary>A custom application error occurred.</summary>
        public const string Code = "CUSTOM_ERROR";

        /// <summary>A business logic validation failed.</summary>
        public const string BusinessLogicError = "CUSTOM_BUSINESS_LOGIC_ERROR";

        /// <summary>An operation is not allowed in the current context.</summary>
        public const string OperationNotAllowed = "CUSTOM_OPERATION_NOT_ALLOWED";
    }
}

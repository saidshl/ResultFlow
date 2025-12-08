using FluentAssertions;
using ResultFlow.Errors;
using ResultFlow.Errors.Builders;
using Xunit;

namespace ResultFlow.Tests.Errors;

/// <summary>
/// Unit tests for Error record and built-in error types.
/// </summary>
public class ErrorTests
{
    #region Base Error

    [Fact]
    public void Error_WithRequiredParameters_CreatesError()
    {
        // Arrange & Act
        var error = new Error("ERR_CODE", "Error message");

        // Assert
        error.Code.Should().Be("ERR_CODE");
        error.Message.Should().Be("Error message");
        error.Details.Should().BeNull();
        error.Metadata.Should().BeNull();
        error.InnerException.Should().BeNull();
    }

    [Fact]
    public void Error_WithAllParameters_CreatesError()
    {
        // Arrange
        var exception = new Exception("Inner");
        var metadata = new Dictionary<string, object> { { "key", "value" } };

        // Act
        var error = new Error(
            "ERR_CODE",
            "Error message",
            "Details",
            metadata,
            exception
        );

        // Assert
        error.Code.Should().Be("ERR_CODE");
        error.Message.Should().Be("Error message");
        error.Details.Should().Be("Details");
        error.Metadata.Should().ContainKey("key");
        error.InnerException.Should().Be(exception);
    }

    #endregion

    #region NotFoundError

    [Fact]
    public void NotFoundError_WithDefaults_CreatesCorrectError()
    {
        // Arrange & Act
        var error = NotFoundError.WithDefaults();

        // Assert
        error.Code.Should().Contain("NOT_FOUND");
        error.Message.Should().Contain("not found");
    }

    [Fact]
    public void NotFoundError_ForResource_IncludesResourceInfo()
    {
        // Arrange & Act
        var error = NotFoundError.ForResource("User", "123");

        // Assert
        error.Message.Should().Contain("User");
        error.Metadata.Should().ContainKey("resourceName");
        error.Metadata!["resourceName"].Should().Be("User");
        error.Metadata.Should().ContainKey("resourceId");
    }

    [Fact]
    public void NotFoundError_ByIdentifier_IncludesIdentifier()
    {
        // Arrange & Act
        var error = NotFoundError.ByIdentifier("Product", 42);

        // Assert
        error.Message.Should().Contain("Product");
        error.Message.Should().Contain("42");
        error.Metadata!["identifier"].Should().Be(42);
    }

    #endregion

    #region BadRequestError

    [Fact]
    public void BadRequestError_WithDefaults_CreatesCorrectError()
    {
        // Arrange & Act
        var error = BadRequestError.WithDefaults("Invalid request");

        // Assert
        error.Code.Should().Contain("BAD_REQUEST");
        error.Message.Should().Be("Invalid request");
    }

    [Fact]
    public void BadRequestError_ForInvalidParameter_IncludesParameterInfo()
    {
        // Arrange & Act
        var error = BadRequestError.ForInvalidParameter("email", "Invalid format", "not-an-email");

        // Assert
        error.Message.Should().Contain("email");
        error.Message.Should().Contain("Invalid format");
        error.Metadata!["parameterName"].Should().Be("email");
        error.Metadata["providedValue"].Should().Be("not-an-email");
    }

    [Fact]
    public void BadRequestError_ForMissingField_IncludesFieldName()
    {
        // Arrange & Act
        var error = BadRequestError.ForMissingField("Username");

        // Assert
        error.Message.Should().Contain("Username");
        error.Message.Should().Contain("required");
        error.Metadata!["fieldName"].Should().Be("Username");
    }

    #endregion

    #region ValidationError

    [Fact]
    public void ValidationError_WithDefaults_CreatesCorrectError()
    {
        // Arrange & Act
        var error = ValidationError.WithDefaults("Validation failed");

        // Assert
        error.Code.Should().NotBeNullOrEmpty();
        error.Message.Should().Be("Validation failed");
    }

    [Fact]
    public void ValidationError_ForField_IncludesFieldInfo()
    {
        // Arrange & Act
        var error = ValidationError.ForField("Age", "Must be at least 18");

        // Assert
        error.Message.Should().Be("Must be at least 18");
        error.Metadata!["field"].Should().Be("Age");
    }

    #endregion

    #region ConflictError

    [Fact]
    public void ConflictError_WithDefaults_CreatesCorrectError()
    {
        // Arrange & Act
        var error = ConflictError.WithDefaults("Conflict occurred");

        // Assert
        error.Code.Should().Contain("CONFLICT");
        error.Message.Should().Be("Conflict occurred");
    }

    [Fact]
    public void ConflictError_ForDuplicateResource_IncludesResourceInfo()
    {
        // Arrange & Act
        var error = ConflictError.ForDuplicateResource("Email", "test@example.com");

        // Assert
        error.Message.Should().Contain("Email");
        error.Message.Should().Contain("test@example.com");
        error.Metadata!["resourceName"].Should().Be("Email");
        error.Metadata["conflictingValue"].Should().Be("test@example.com");
    }

    [Fact]
    public void ConflictError_ForVersionMismatch_IncludesVersionInfo()
    {
        // Arrange & Act
        var error = ConflictError.ForVersionMismatch("Document", "v1", "v2");

        // Assert
        error.Message.Should().Contain("Document");
        error.Message.Should().Contain("v1");
        error.Message.Should().Contain("v2");
    }

    #endregion

    #region UnauthorizedError

    [Fact]
    public void UnauthorizedError_WithDefaults_CreatesCorrectError()
    {
        // Arrange & Act
        var error = UnauthorizedError.WithDefaults();

        // Assert
        error.Code.Should().Contain("UNAUTHORIZED");
        error.Message.Should().Contain("authentication");
    }

    [Fact]
    public void UnauthorizedError_ForReason_IncludesReason()
    {
        // Arrange & Act
        var error = UnauthorizedError.ForReason("Token expired");

        // Assert
        error.Message.Should().Contain("Token expired");
        error.Metadata!["reason"].Should().Be("Token expired");
    }

    #endregion

    #region ForbiddenError

    [Fact]
    public void ForbiddenError_WithDefaults_CreatesCorrectError()
    {
        // Arrange & Act
        var error = ForbiddenError.WithDefaults();

        // Assert
        error.Code.Should().Contain("FORBIDDEN");
        error.Message.Should().Contain("permission");
    }

    [Fact]
    public void ForbiddenError_ForMissingRole_IncludesRoleInfo()
    {
        // Arrange & Act
        var error = ForbiddenError.ForMissingRole("Admin", "User");

        // Assert
        error.Message.Should().Contain("Admin");
        error.Metadata!["requiredRole"].Should().Be("Admin");
        error.Metadata["userRole"].Should().Be("User");
    }

    [Fact]
    public void ForbiddenError_ForMissingPermission_IncludesPermission()
    {
        // Arrange & Act
        var error = ForbiddenError.ForMissingPermission("delete:users");

        // Assert
        error.Message.Should().Contain("delete:users");
        error.Metadata!["requiredPermission"].Should().Be("delete:users");
    }

    #endregion

    #region InternalServerError

    [Fact]
    public void InternalServerError_WithDefaults_CreatesCorrectError()
    {
        // Arrange & Act
        var error = InternalServerError.WithDefaults();

        // Assert
        error.Code.Should().NotBeNullOrEmpty();
        error.Message.Should().Contain("internal server error");
    }

    [Fact]
    public void InternalServerError_FromException_IncludesExceptionInfo()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        // Act
        var error = InternalServerError.FromException(exception);

        // Assert
        error.InnerException.Should().Be(exception);
        error.Metadata.Should().ContainKey("exceptionType");
        error.Metadata!["exceptionType"].ToString().Should().Contain("InvalidOperationException");
    }

    [Fact]
    public void InternalServerError_ForOperation_IncludesOperationName()
    {
        // Arrange
        var exception = new Exception("DB error");

        // Act
        var error = InternalServerError.ForOperation("SaveUser", exception);

        // Assert
        error.Message.Should().Contain("SaveUser");
        error.Metadata!["operation"].Should().Be("SaveUser");
    }

    #endregion

    #region TooManyRequestsError

    [Fact]
    public void TooManyRequestsError_WithDefaults_CreatesCorrectError()
    {
        // Arrange & Act
        var error = TooManyRequestsError.WithDefaults();

        // Assert
        error.Code.Should().Contain("TOO_MANY_REQUESTS");
    }

    #endregion
}

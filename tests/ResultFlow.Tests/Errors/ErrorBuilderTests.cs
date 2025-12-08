using FluentAssertions;
using ResultFlow.Errors;
using ResultFlow.Errors.Builders;
using Xunit;

namespace ResultFlow.Tests.Errors;

/// <summary>
/// Unit tests for ErrorBuilder class.
/// </summary>
public class ErrorBuilderTests
{
    [Fact]
    public void Create_WithCodeAndMessage_BuildsError()
    {
        // Arrange & Act
        var error = ErrorBuilder
            .Create("ERR_CODE", "Error message")
            .Build();

        // Assert
        error.Code.Should().Be("ERR_CODE");
        error.Message.Should().Be("Error message");
    }

    [Fact]
    public void WithDetails_AddsDetails()
    {
        // Arrange & Act
        var error = ErrorBuilder
            .Create("ERR", "Message")
            .WithDetails("Additional details")
            .Build();

        // Assert
        error.Details.Should().Be("Additional details");
    }

    [Fact]
    public void AddMetadata_AddsKeyValuePair()
    {
        // Arrange & Act
        var error = ErrorBuilder
            .Create("ERR", "Message")
            .AddMetadata("userId", 123)
            .AddMetadata("timestamp", "2024-01-01")
            .Build();

        // Assert
        error.Metadata.Should().ContainKey("userId");
        error.Metadata!["userId"].Should().Be(123);
        error.Metadata.Should().ContainKey("timestamp");
    }

    [Fact]
    public void WithException_AddsException()
    {
        // Arrange
        var exception = new InvalidOperationException("Test");

        // Act
        var error = ErrorBuilder
            .Create("ERR", "Message")
            .WithException(exception)
            .Build();

        // Assert
        error.InnerException.Should().Be(exception);
    }

    [Fact]
    public void Build_ChainedMethods_CreatesCompleteError()
    {
        // Arrange
        var exception = new Exception("Inner error");

        // Act
        var error = ErrorBuilder
            .Create("PAYMENT_FAILED", "Payment processing failed")
            .WithDetails("Credit card was declined")
            .AddMetadata("orderId", "ORD-123")
            .AddMetadata("amount", 99.99m)
            .AddMetadata("gateway", "Stripe")
            .WithException(exception)
            .Build();

        // Assert
        error.Code.Should().Be("PAYMENT_FAILED");
        error.Message.Should().Be("Payment processing failed");
        error.Details.Should().Be("Credit card was declined");
        error.Metadata.Should().ContainKey("orderId");
        error.Metadata!["orderId"].Should().Be("ORD-123");
        error.Metadata["amount"].Should().Be(99.99m);
        error.InnerException.Should().Be(exception);
    }

    [Fact]
    public void Empty_CreatesEmptyBuilder()
    {
        // Arrange & Act
        var error = ErrorBuilder
            .Empty()
            .WithCode("CODE")
            .WithMessage("Message")
            .Build();

        // Assert
        error.Code.Should().Be("CODE");
        error.Message.Should().Be("Message");
    }

    [Fact]
    public void WithCode_OverwritesExistingCode()
    {
        // Arrange & Act
        var error = ErrorBuilder
            .Create("OLD_CODE", "Message")
            .WithCode("NEW_CODE")
            .Build();

        // Assert
        error.Code.Should().Be("NEW_CODE");
    }

    [Fact]
    public void WithMessage_OverwritesExistingMessage()
    {
        // Arrange & Act
        var error = ErrorBuilder
            .Create("CODE", "Old message")
            .WithMessage("New message")
            .Build();

        // Assert
        error.Message.Should().Be("New message");
    }

    [Fact]
    public void AddMetadataRange_AddsDictionaryToMetadata()
    {
        // Arrange
        var additionalData = new Dictionary<string, object>
        {
            { "key1", "value1" },
            { "key2", 42 }
        };

        // Act
        var error = ErrorBuilder
            .Create("ERR", "Message")
            .AddMetadata("existing", "data")
            .AddMetadataRange(additionalData)
            .Build();

        // Assert
        error.Metadata.Should().HaveCount(3);
        error.Metadata.Should().ContainKey("existing");
        error.Metadata.Should().ContainKey("key1");
        error.Metadata.Should().ContainKey("key2");
    }

    [Fact]
    public void ClearMetadata_RemovesAllMetadata()
    {
        // Arrange & Act
        var error = ErrorBuilder
            .Create("ERR", "Message")
            .AddMetadata("key1", "value1")
            .AddMetadata("key2", "value2")
            .ClearMetadata()
            .Build();

        // Assert
        error.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Build_Generic_CreatesTypedError()
    {
        // Arrange & Act
        var error = ErrorBuilder
            .Create("NOT_FOUND", "Resource not found")
            .AddMetadata("resourceName", "User")
            .Build<NotFoundError>();

        // Assert
        error.Should().BeOfType<NotFoundError>();
        error.Code.Should().Be("NOT_FOUND");
    }

    [Fact]
    public void WithMetadata_ReplacesAllMetadata()
    {
        // Arrange
        var metadata = new Dictionary<string, object>
        {
            { "newKey", "newValue" }
        };

        // Act
        var error = ErrorBuilder
            .Create("ERR", "Message")
            .AddMetadata("oldKey", "oldValue")
            .WithMetadata(metadata)
            .Build();

        // Assert
        error.Metadata.Should().HaveCount(1);
        error.Metadata.Should().ContainKey("newKey");
        error.Metadata.Should().NotContainKey("oldKey");
    }
}

using FluentAssertions;
using ResultFlow.Errors;
using ResultFlow.Results;
using Xunit;

namespace ResultFlow.Tests.Results;

/// <summary>
/// Unit tests for VoidResult struct.
/// </summary>
public class VoidResultTests
{
    #region Factory Methods

    [Fact]
    public void Ok_CreatesSuccessResult()
    {
        // Arrange & Act
        var result = VoidResult.Ok();

        // Assert
        result.IsOk.Should().BeTrue();
        result.HasError.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failed_WithError_CreatesFailureResult()
    {
        // Arrange
        var error = new Error("TEST_ERROR", "Test error message");

        // Act
        var result = VoidResult.Failed(error);

        // Assert
        result.IsOk.Should().BeFalse();
        result.HasError.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Failed_WithNullError_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var action = () => VoidResult.Failed(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Static Result Class

    [Fact]
    public void Result_Success_CreatesSuccessVoidResult()
    {
        // Arrange & Act
        var result = Result.Success();

        // Assert
        result.IsOk.Should().BeTrue();
    }

    [Fact]
    public void Result_Ok_CreatesSuccessVoidResult()
    {
        // Arrange & Act
        var result = Result.Ok();

        // Assert
        result.IsOk.Should().BeTrue();
    }

    [Fact]
    public void Result_Failure_CreatesFailureVoidResult()
    {
        // Arrange
        var error = new Error("ERR", "Error");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.HasError.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Result_Failed_CreatesFailureVoidResult()
    {
        // Arrange
        var error = new Error("ERR", "Error");

        // Act
        var result = Result.Failed(error);

        // Assert
        result.HasError.Should().BeTrue();
    }

    #endregion

    #region Implicit Conversion

    [Fact]
    public void ImplicitConversion_FromError_CreatesFailureResult()
    {
        // Arrange
        var error = new Error("TEST", "Test");

        // Act
        VoidResult result = error;

        // Assert
        result.HasError.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    #endregion

    #region Match

    [Fact]
    public void Match_WithSuccessResult_ExecutesOnSuccess()
    {
        // Arrange
        var result = VoidResult.Ok();
        var successCalled = false;
        var failureCalled = false;

        // Act
        result.Match(
            onSuccess: () => successCalled = true,
            onFailure: _ => failureCalled = true
        );

        // Assert
        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
    }

    [Fact]
    public void Match_WithFailureResult_ExecutesOnFailure()
    {
        // Arrange
        var error = new Error("ERR", "Error");
        var result = VoidResult.Failed(error);
        var successCalled = false;
        Error? capturedError = null;

        // Act
        result.Match(
            onSuccess: () => successCalled = true,
            onFailure: e => capturedError = e
        );

        // Assert
        successCalled.Should().BeFalse();
        capturedError.Should().Be(error);
    }

    [Fact]
    public void Match_WithReturnValue_ReturnsCorrectValue()
    {
        // Arrange
        var result = VoidResult.Ok();

        // Act
        var output = result.Match(
            onSuccess: () => "Success!",
            onFailure: e => $"Error: {e.Message}"
        );

        // Assert
        output.Should().Be("Success!");
    }

    [Fact]
    public void Match_WithFailure_ReturnsErrorValue()
    {
        // Arrange
        var result = VoidResult.Failed(new Error("ERR", "Something failed"));

        // Act
        var output = result.Match(
            onSuccess: () => "Success!",
            onFailure: e => $"Error: {e.Message}"
        );

        // Assert
        output.Should().Be("Error: Something failed");
    }

    #endregion

    #region Tap

    [Fact]
    public void Tap_WithSuccessResult_ExecutesSideEffect()
    {
        // Arrange
        var result = VoidResult.Ok();
        var sideEffectExecuted = false;

        // Act
        var tapped = result.Tap(() => sideEffectExecuted = true);

        // Assert
        sideEffectExecuted.Should().BeTrue();
        tapped.Should().Be(result);
    }

    [Fact]
    public void Tap_WithFailureResult_DoesNotExecuteSideEffect()
    {
        // Arrange
        var result = VoidResult.Failed(new Error("ERR", "Error"));
        var sideEffectExecuted = false;

        // Act
        var tapped = result.Tap(() => sideEffectExecuted = true);

        // Assert
        sideEffectExecuted.Should().BeFalse();
        tapped.Should().Be(result);
    }

    [Fact]
    public void TapError_WithFailureResult_ExecutesSideEffect()
    {
        // Arrange
        var error = new Error("ERR", "Error message");
        var result = VoidResult.Failed(error);
        string? capturedMessage = null;

        // Act
        var tapped = result.TapError(e => capturedMessage = e.Message);

        // Assert
        capturedMessage.Should().Be("Error message");
        tapped.Should().Be(result);
    }

    [Fact]
    public void TapError_WithSuccessResult_DoesNotExecuteSideEffect()
    {
        // Arrange
        var result = VoidResult.Ok();
        var sideEffectExecuted = false;

        // Act
        var tapped = result.TapError(_ => sideEffectExecuted = true);

        // Assert
        sideEffectExecuted.Should().BeFalse();
    }

    #endregion

    #region Then

    [Fact]
    public void Then_VoidResult_WithSuccess_ReturnsSecondResult()
    {
        // Arrange
        var first = VoidResult.Ok();
        var second = VoidResult.Ok();

        // Act
        var combined = first.Then(second);

        // Assert
        combined.IsOk.Should().BeTrue();
    }

    [Fact]
    public void Then_VoidResult_WithFailedFirst_ReturnsFirstError()
    {
        // Arrange
        var error = new Error("FIRST", "First error");
        var first = VoidResult.Failed(error);
        var second = VoidResult.Ok();

        // Act
        var combined = first.Then(second);

        // Assert
        combined.HasError.Should().BeTrue();
        combined.Error.Should().Be(error);
    }

    [Fact]
    public void Then_WithFunction_ExecutesFunction()
    {
        // Arrange
        var result = VoidResult.Ok();

        // Act
        var chained = result.Then(() => VoidResult.Ok());

        // Assert
        chained.IsOk.Should().BeTrue();
    }

    [Fact]
    public void Then_WithFunction_WhenFailed_DoesNotExecuteFunction()
    {
        // Arrange
        var error = new Error("ERR", "Error");
        var result = VoidResult.Failed(error);
        var functionCalled = false;

        // Act
        var chained = result.Then(() =>
        {
            functionCalled = true;
            return VoidResult.Ok();
        });

        // Assert
        functionCalled.Should().BeFalse();
        chained.Error.Should().Be(error);
    }

    [Fact]
    public void Then_GenericResult_WithSuccess_ReturnsSecondResult()
    {
        // Arrange
        var first = VoidResult.Ok();
        var second = Result<int>.Ok(42);

        // Act
        var combined = first.Then(second);

        // Assert
        combined.IsOk.Should().BeTrue();
        combined.Value.Should().Be(42);
    }

    [Fact]
    public void Then_GenericResult_WithFailedFirst_ReturnsFirstError()
    {
        // Arrange
        var error = new Error("FIRST", "First error");
        var first = VoidResult.Failed(error);
        var second = Result<int>.Ok(42);

        // Act
        var combined = first.Then(second);

        // Assert
        combined.HasError.Should().BeTrue();
        combined.Error.Should().Be(error);
    }

    [Fact]
    public void Then_GenericResultFunction_ExecutesFunction()
    {
        // Arrange
        var result = VoidResult.Ok();

        // Act
        var chained = result.Then(() => Result<string>.Ok("Hello"));

        // Assert
        chained.IsOk.Should().BeTrue();
        chained.Value.Should().Be("Hello");
    }

    #endregion

    #region GetErrorOrThrow

    [Fact]
    public void GetErrorOrThrow_WithFailureResult_ReturnsError()
    {
        // Arrange
        var error = new Error("ERR", "Error");
        var result = VoidResult.Failed(error);

        // Act
        var returnedError = result.GetErrorOrThrow();

        // Assert
        returnedError.Should().Be(error);
    }

    [Fact]
    public void GetErrorOrThrow_WithSuccessResult_ThrowsException()
    {
        // Arrange
        var result = VoidResult.Ok();

        // Act
        var action = () => result.GetErrorOrThrow();

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*succeeded*");
    }

    #endregion

    #region Equality

    [Fact]
    public void Equals_TwoSuccessResults_ReturnsTrue()
    {
        // Arrange
        var result1 = VoidResult.Ok();
        var result2 = VoidResult.Ok();

        // Assert
        result1.Equals(result2).Should().BeTrue();
        (result1 == result2).Should().BeTrue();
    }

    [Fact]
    public void Equals_TwoFailuresWithSameError_ReturnsTrue()
    {
        // Arrange
        var result1 = VoidResult.Failed(new Error("ERR", "Error"));
        var result2 = VoidResult.Failed(new Error("ERR", "Error"));

        // Assert
        result1.Equals(result2).Should().BeTrue();
    }

    [Fact]
    public void Equals_SuccessAndFailure_ReturnsFalse()
    {
        // Arrange
        var success = VoidResult.Ok();
        var failure = VoidResult.Failed(new Error("ERR", "Error"));

        // Assert
        success.Equals(failure).Should().BeFalse();
        (success != failure).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_SameResults_ReturnsSameHashCode()
    {
        // Arrange
        var result1 = VoidResult.Ok();
        var result2 = VoidResult.Ok();

        // Assert
        result1.GetHashCode().Should().Be(result2.GetHashCode());
    }

    #endregion

    #region ToString

    [Fact]
    public void ToString_WithSuccessResult_ReturnsSuccessString()
    {
        // Arrange
        var result = VoidResult.Ok();

        // Act
        var str = result.ToString();

        // Assert
        str.Should().Be("Success()");
    }

    [Fact]
    public void ToString_WithFailureResult_ReturnsFailureString()
    {
        // Arrange
        var result = VoidResult.Failed(new Error("ERR_CODE", "Error message"));

        // Act
        var str = result.ToString();

        // Assert
        str.Should().Be("Failure(ERR_CODE: Error message)");
    }

    #endregion
}

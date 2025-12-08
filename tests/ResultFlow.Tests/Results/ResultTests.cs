using FluentAssertions;
using ResultFlow.Errors;
using ResultFlow.Results;
using Xunit;

namespace ResultFlow.Tests.Results;

/// <summary>
/// Unit tests for Result{T} struct.
/// </summary>
public class ResultTests
{
    #region Factory Methods

    [Fact]
    public void Ok_WithValue_CreatesSuccessResult()
    {
        // Arrange & Act
        var result = Result<int>.Ok(42);

        // Assert
        result.IsOk.Should().BeTrue();
        result.HasError.Should().BeFalse();
        result.Value.Should().Be(42);
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Success_WithValue_CreatesSuccessResult()
    {
        // Arrange & Act
        var result = Result<string>.Success("test");

        // Assert
        result.IsOk.Should().BeTrue();
        result.Value.Should().Be("test");
    }

    [Fact]
    public void Failed_WithError_CreatesFailureResult()
    {
        // Arrange
        var error = new Error("TEST_ERROR", "Test error message");

        // Act
        var result = Result<int>.Failed(error);

        // Assert
        result.IsOk.Should().BeFalse();
        result.HasError.Should().BeTrue();
        result.Error.Should().Be(error);
        result.Value.Should().Be(default);
    }

    [Fact]
    public void Failure_WithError_CreatesFailureResult()
    {
        // Arrange
        var error = new Error("TEST_ERROR", "Test error message");

        // Act
        var result = Result<int>.Failure(error);

        // Assert
        result.HasError.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Failed_WithNullError_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var action = () => Result<int>.Failed(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Ok_WithNullValue_CreatesSuccessResultWithNull()
    {
        // Arrange & Act
        var result = Result<string?>.Ok(null);

        // Assert
        result.IsOk.Should().BeTrue();
        result.Value.Should().BeNull();
        result.HasValue.Should().BeFalse();
    }

    #endregion

    #region Implicit Conversions

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccessResult()
    {
        // Arrange & Act
        Result<int> result = 42;

        // Assert
        result.IsOk.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ImplicitConversion_FromError_CreatesFailureResult()
    {
        // Arrange
        var error = new Error("TEST", "Test");

        // Act
        Result<int> result = error;

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
        var result = Result<int>.Ok(42);

        // Act
        var output = result.Match(
            onSuccess: value => $"Value: {value}",
            onFailure: error => $"Error: {error.Message}"
        );

        // Assert
        output.Should().Be("Value: 42");
    }

    [Fact]
    public void Match_WithFailureResult_ExecutesOnFailure()
    {
        // Arrange
        var error = new Error("ERR", "Something went wrong");
        var result = Result<int>.Failed(error);

        // Act
        var output = result.Match(
            onSuccess: value => $"Value: {value}",
            onFailure: err => $"Error: {err.Message}"
        );

        // Assert
        output.Should().Be("Error: Something went wrong");
    }

    [Fact]
    public void Match_WithAction_ExecutesCorrectAction()
    {
        // Arrange
        var result = Result<int>.Ok(42);
        var successCalled = false;
        var failureCalled = false;

        // Act
        result.Match(
            onSuccess: _ => successCalled = true,
            onFailure: _ => failureCalled = true
        );

        // Assert
        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
    }

    #endregion

    #region Map

    [Fact]
    public void Map_WithSuccessResult_TransformsValue()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act
        var mapped = result.Map(x => x * 2);

        // Assert
        mapped.IsOk.Should().BeTrue();
        mapped.Value.Should().Be(10);
    }

    [Fact]
    public void Map_WithFailureResult_PropagatesError()
    {
        // Arrange
        var error = new Error("ERR", "Error");
        var result = Result<int>.Failed(error);

        // Act
        var mapped = result.Map(x => x * 2);

        // Assert
        mapped.HasError.Should().BeTrue();
        mapped.Error.Should().Be(error);
    }

    [Fact]
    public void Map_ChainedTransformations_WorksCorrectly()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act
        var mapped = result
            .Map(x => x * 2)
            .Map(x => x.ToString())
            .Map(x => $"Result: {x}");

        // Assert
        mapped.IsOk.Should().BeTrue();
        mapped.Value.Should().Be("Result: 10");
    }

    #endregion

    #region Bind

    [Fact]
    public void Bind_WithSuccessResult_ExecutesBindFunction()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act
        var bound = result.Bind(x => Result<string>.Ok($"Value: {x}"));

        // Assert
        bound.IsOk.Should().BeTrue();
        bound.Value.Should().Be("Value: 5");
    }

    [Fact]
    public void Bind_WithFailureResult_PropagatesError()
    {
        // Arrange
        var error = new Error("ERR", "Error");
        var result = Result<int>.Failed(error);

        // Act
        var bound = result.Bind(x => Result<string>.Ok($"Value: {x}"));

        // Assert
        bound.HasError.Should().BeTrue();
        bound.Error.Should().Be(error);
    }

    [Fact]
    public void Bind_WhenBindFunctionReturnsFailure_PropagatesNewError()
    {
        // Arrange
        var result = Result<int>.Ok(5);
        var newError = new Error("NEW_ERR", "New error");

        // Act
        var bound = result.Bind<string>(_ => Result<string>.Failed(newError));

        // Assert
        bound.HasError.Should().BeTrue();
        bound.Error.Should().Be(newError);
    }

    #endregion

    #region Filter

    [Fact]
    public void Filter_WhenPredicateIsTrue_ReturnsOriginalResult()
    {
        // Arrange
        var result = Result<int>.Ok(42);
        var error = new Error("FILTER_ERR", "Filter failed");

        // Act
        var filtered = result.Filter(x => x > 0, error);

        // Assert
        filtered.IsOk.Should().BeTrue();
        filtered.Value.Should().Be(42);
    }

    [Fact]
    public void Filter_WhenPredicateIsFalse_ReturnsFailure()
    {
        // Arrange
        var result = Result<int>.Ok(-5);
        var error = new Error("FILTER_ERR", "Value must be positive");

        // Act
        var filtered = result.Filter(x => x > 0, error);

        // Assert
        filtered.HasError.Should().BeTrue();
        filtered.Error.Should().Be(error);
    }

    [Fact]
    public void Filter_WhenAlreadyFailed_ReturnsOriginalError()
    {
        // Arrange
        var originalError = new Error("ORIGINAL", "Original error");
        var filterError = new Error("FILTER", "Filter error");
        var result = Result<int>.Failed(originalError);

        // Act
        var filtered = result.Filter(x => x > 0, filterError);

        // Assert
        filtered.HasError.Should().BeTrue();
        filtered.Error.Should().Be(originalError);
    }

    #endregion

    #region Tap

    [Fact]
    public void Tap_WithSuccessResult_ExecutesSideEffect()
    {
        // Arrange
        var result = Result<int>.Ok(42);
        var sideEffectValue = 0;

        // Act
        var tapped = result.Tap(x => sideEffectValue = x);

        // Assert
        sideEffectValue.Should().Be(42);
        tapped.Should().Be(result);
    }

    [Fact]
    public void Tap_WithFailureResult_DoesNotExecuteSideEffect()
    {
        // Arrange
        var result = Result<int>.Failed(new Error("ERR", "Error"));
        var sideEffectExecuted = false;

        // Act
        var tapped = result.Tap(_ => sideEffectExecuted = true);

        // Assert
        sideEffectExecuted.Should().BeFalse();
        tapped.Should().Be(result);
    }

    [Fact]
    public void TapError_WithFailureResult_ExecutesSideEffect()
    {
        // Arrange
        var error = new Error("ERR", "Error message");
        var result = Result<int>.Failed(error);
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
        var result = Result<int>.Ok(42);
        var sideEffectExecuted = false;

        // Act
        var tapped = result.TapError(_ => sideEffectExecuted = true);

        // Assert
        sideEffectExecuted.Should().BeFalse();
    }

    #endregion

    #region Then

    [Fact]
    public void Then_WithSuccessResult_ReturnsSecondResult()
    {
        // Arrange
        var first = Result<int>.Ok(1);
        var second = Result<string>.Ok("second");

        // Act
        var combined = first.Then(second);

        // Assert
        combined.IsOk.Should().BeTrue();
        combined.Value.Should().Be("second");
    }

    [Fact]
    public void Then_WithFailedFirstResult_ReturnsFirstError()
    {
        // Arrange
        var error = new Error("FIRST_ERR", "First error");
        var first = Result<int>.Failed(error);
        var second = Result<string>.Ok("second");

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
        var result = Result<int>.Ok(5);

        // Act
        var chained = result.Then(x => Result<string>.Ok($"Value: {x}"));

        // Assert
        chained.IsOk.Should().BeTrue();
        chained.Value.Should().Be("Value: 5");
    }

    #endregion

    #region GetValueOr Methods

    [Fact]
    public void GetValueOrDefault_WithSuccessResult_ReturnsValue()
    {
        // Arrange
        var result = Result<int>.Ok(42);

        // Act
        var value = result.GetValueOrDefault(0);

        // Assert
        value.Should().Be(42);
    }

    [Fact]
    public void GetValueOrDefault_WithFailureResult_ReturnsDefault()
    {
        // Arrange
        var result = Result<int>.Failed(new Error("ERR", "Error"));

        // Act
        var value = result.GetValueOrDefault(99);

        // Assert
        value.Should().Be(99);
    }

    [Fact]
    public void GetValueOrElse_WithFailureResult_ExecutesFallback()
    {
        // Arrange
        var error = new Error("ERR", "Error");
        var result = Result<int>.Failed(error);

        // Act
        var value = result.GetValueOrElse(e => e.Code == "ERR" ? -1 : 0);

        // Assert
        value.Should().Be(-1);
    }

    [Fact]
    public void GetValueOrThrow_WithSuccessResult_ReturnsValue()
    {
        // Arrange
        var result = Result<int>.Ok(42);

        // Act
        var value = result.GetValueOrThrow();

        // Assert
        value.Should().Be(42);
    }

    [Fact]
    public void GetValueOrThrow_WithFailureResult_ThrowsException()
    {
        // Arrange
        var result = Result<int>.Failed(new Error("ERR", "Something failed"));

        // Act
        var action = () => result.GetValueOrThrow();

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Something failed*");
    }

    [Fact]
    public void GetValueOrThrow_WithCustomException_ThrowsCustomException()
    {
        // Arrange
        var result = Result<int>.Failed(new Error("ERR", "Error"));

        // Act
        var action = () => result.GetValueOrThrow(e => new ArgumentException(e.Message));

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Equality

    [Fact]
    public void Equals_WithSameSuccessValue_ReturnsTrue()
    {
        // Arrange
        var result1 = Result<int>.Ok(42);
        var result2 = Result<int>.Ok(42);

        // Assert
        result1.Equals(result2).Should().BeTrue();
        (result1 == result2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentSuccessValues_ReturnsFalse()
    {
        // Arrange
        var result1 = Result<int>.Ok(42);
        var result2 = Result<int>.Ok(43);

        // Assert
        result1.Equals(result2).Should().BeFalse();
        (result1 != result2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithSameError_ReturnsTrue()
    {
        // Arrange
        var result1 = Result<int>.Failed(new Error("ERR", "Error"));
        var result2 = Result<int>.Failed(new Error("ERR", "Error"));

        // Assert
        result1.Equals(result2).Should().BeTrue();
    }

    [Fact]
    public void Equals_SuccessAndFailure_ReturnsFalse()
    {
        // Arrange
        var success = Result<int>.Ok(42);
        var failure = Result<int>.Failed(new Error("ERR", "Error"));

        // Assert
        success.Equals(failure).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_SameResults_ReturnsSameHashCode()
    {
        // Arrange
        var result1 = Result<int>.Ok(42);
        var result2 = Result<int>.Ok(42);

        // Assert
        result1.GetHashCode().Should().Be(result2.GetHashCode());
    }

    #endregion

    #region ToString

    [Fact]
    public void ToString_WithSuccessResult_ReturnsSuccessString()
    {
        // Arrange
        var result = Result<int>.Ok(42);

        // Act
        var str = result.ToString();

        // Assert
        str.Should().Contain("42");
    }

    [Fact]
    public void ToString_WithFailureResult_ReturnsFailureString()
    {
        // Arrange
        var result = Result<int>.Failed(new Error("ERR_CODE", "Error message"));

        // Act
        var str = result.ToString();

        // Assert
        str.Should().Contain("Failed");
        str.Should().Contain("ERR_CODE");
    }

    #endregion
}

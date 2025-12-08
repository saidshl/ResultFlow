using FluentAssertions;
using ResultFlow.Errors;
using ResultFlow.Results;
using Xunit;

namespace ResultFlow.Tests.Results;

/// <summary>
/// Unit tests for Result static class methods.
/// </summary>
public class ResultStaticTests
{
    #region Try - Sync

    [Fact]
    public void Try_WithSuccessfulOperation_ReturnsSuccessResult()
    {
        // Arrange & Act
        var result = Result.Try(() => 42);

        // Assert
        result.IsOk.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Try_WithThrowingOperation_ReturnsFailureResult()
    {
        // Arrange & Act
        var result = Result.Try<int>(() => throw new InvalidOperationException("Test exception"));

        // Assert
        result.HasError.Should().BeTrue();
        result.Error.Should().BeOfType<InternalServerError>();
    }

    [Fact]
    public void Try_WithCustomErrorFactory_UsesCustomError()
    {
        // Arrange
        var customError = new Error("CUSTOM", "Custom error");

        // Act
        var result = Result.Try<int>(
            () => throw new Exception("Test"),
            _ => customError
        );

        // Assert
        result.HasError.Should().BeTrue();
        result.Error.Should().Be(customError);
    }

    [Fact]
    public void Try_VoidOperation_WithSuccess_ReturnsSuccessResult()
    {
        // Arrange
        var executed = false;

        // Act
        var result = Result.Try(() => executed = true);

        // Assert
        result.IsOk.Should().BeTrue();
        executed.Should().BeTrue();
    }

    [Fact]
    public void Try_VoidOperation_WithException_ReturnsFailureResult()
    {
        // Arrange & Act
        var result = Result.Try(() => throw new Exception("Test"));

        // Assert
        result.HasError.Should().BeTrue();
    }

    #endregion

    #region TryAsync

    [Fact]
    public async Task TryAsync_WithSuccessfulOperation_ReturnsSuccessResult()
    {
        // Arrange & Act
        var result = await Result.TryAsync(async () =>
        {
            await Task.Delay(1);
            return 42;
        });

        // Assert
        result.IsOk.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task TryAsync_WithThrowingOperation_ReturnsFailureResult()
    {
        // Arrange & Act
        var result = await Result.TryAsync<int>(async () =>
        {
            await Task.Delay(1);
            throw new InvalidOperationException("Async exception");
        });

        // Assert
        result.HasError.Should().BeTrue();
        result.Error.Should().BeOfType<InternalServerError>();
    }

    [Fact]
    public async Task TryAsync_WithCustomErrorFactory_UsesCustomError()
    {
        // Arrange
        var customError = new Error("ASYNC_ERR", "Async error");

        // Act
        var result = await Result.TryAsync<int>(
            async () =>
            {
                await Task.Delay(1);
                throw new Exception("Test");
            },
            _ => customError
        );

        // Assert
        result.Error.Should().Be(customError);
    }

    [Fact]
    public async Task TryAsync_VoidOperation_WithSuccess_ReturnsSuccessResult()
    {
        // Arrange
        var executed = false;

        // Act
        var result = await Result.TryAsync(async () =>
        {
            await Task.Delay(1);
            executed = true;
        });

        // Assert
        result.IsOk.Should().BeTrue();
        executed.Should().BeTrue();
    }

    [Fact]
    public async Task TryAsync_VoidOperation_WithException_ReturnsFailureResult()
    {
        // Arrange & Act
        var result = await Result.TryAsync(async () =>
        {
            await Task.Delay(1);
            throw new Exception("Test");
        });

        // Assert
        result.HasError.Should().BeTrue();
    }

    #endregion

    #region Combine

    [Fact]
    public void Combine_AllSuccess_ReturnsCombinedValues()
    {
        // Arrange
        var result1 = Result<int>.Ok(1);
        var result2 = Result<int>.Ok(2);
        var result3 = Result<int>.Ok(3);

        // Act
        var combined = Result.Combine(result1, result2, result3);

        // Assert
        combined.IsOk.Should().BeTrue();
        combined.Value.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public void Combine_WithOneFailure_ReturnsFirstError()
    {
        // Arrange
        var error = new Error("ERR", "Error in second");
        var result1 = Result<int>.Ok(1);
        var result2 = Result<int>.Failed(error);
        var result3 = Result<int>.Ok(3);

        // Act
        var combined = Result.Combine(result1, result2, result3);

        // Assert
        combined.HasError.Should().BeTrue();
        combined.Error.Should().Be(error);
    }

    [Fact]
    public void Combine_WithMultipleFailures_ReturnsFirstError()
    {
        // Arrange
        var error1 = new Error("ERR1", "First error");
        var error2 = new Error("ERR2", "Second error");
        var result1 = Result<int>.Failed(error1);
        var result2 = Result<int>.Failed(error2);

        // Act
        var combined = Result.Combine(result1, result2);

        // Assert
        combined.HasError.Should().BeTrue();
        combined.Error.Should().Be(error1);
    }

    [Fact]
    public void Combine_EmptyArray_ReturnsEmptyList()
    {
        // Arrange & Act
        var combined = Result.Combine<int>();

        // Assert
        combined.IsOk.Should().BeTrue();
        combined.Value.Should().BeEmpty();
    }

    [Fact]
    public void Combine_WithEnumerable_WorksCorrectly()
    {
        // Arrange
        var results = new List<Result<int>>
        {
            Result<int>.Ok(1),
            Result<int>.Ok(2),
            Result<int>.Ok(3)
        };

        // Act
        var combined = Result.Combine(results);

        // Assert
        combined.IsOk.Should().BeTrue();
        combined.Value.Should().HaveCount(3);
    }

    [Fact]
    public void Combine_WithEnumerable_FailsOnError()
    {
        // Arrange
        var error = new Error("ERR", "Error");
        var results = new List<Result<int>>
        {
            Result<int>.Ok(1),
            Result<int>.Failed(error),
            Result<int>.Ok(3)
        };

        // Act
        var combined = Result.Combine(results);

        // Assert
        combined.HasError.Should().BeTrue();
        combined.Error.Should().Be(error);
    }

    #endregion
}

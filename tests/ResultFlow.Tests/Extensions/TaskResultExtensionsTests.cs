using FluentAssertions;
using ResultFlow.Errors;
using ResultFlow.Extensions;
using ResultFlow.Results;
using Xunit;

namespace ResultFlow.Tests.Extensions;

/// <summary>
/// Unit tests for Task{Result{T}} extension methods.
/// </summary>
public class TaskResultExtensionsTests
{
    #region MapAsync

    [Fact]
    public async Task MapAsync_WithSuccessResult_TransformsValue()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(5));

        // Act
        var mapped = await resultTask.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        // Assert
        mapped.IsOk.Should().BeTrue();
        mapped.Value.Should().Be(10);
    }

    [Fact]
    public async Task MapAsync_WithFailureResult_PropagatesError()
    {
        // Arrange
        var error = new Error("ERR", "Error");
        var resultTask = Task.FromResult(Result<int>.Failed(error));

        // Act
        var mapped = await resultTask.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        // Assert
        mapped.HasError.Should().BeTrue();
        mapped.Error.Should().Be(error);
    }

    [Fact]
    public async Task Map_Sync_WithSuccessResult_TransformsValue()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(5));

        // Act
        var mapped = await resultTask.Map(x => x * 2);

        // Assert
        mapped.Value.Should().Be(10);
    }

    #endregion

    #region BindAsync

    [Fact]
    public async Task BindAsync_WithSuccessResult_ExecutesBindFunction()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(5));

        // Act
        var bound = await resultTask.BindAsync(async x =>
        {
            await Task.Delay(1);
            return Result<string>.Ok($"Value: {x}");
        });

        // Assert
        bound.IsOk.Should().BeTrue();
        bound.Value.Should().Be("Value: 5");
    }

    [Fact]
    public async Task BindAsync_WithFailureResult_PropagatesError()
    {
        // Arrange
        var error = new Error("ERR", "Error");
        var resultTask = Task.FromResult(Result<int>.Failed(error));

        // Act
        var bound = await resultTask.BindAsync(async x =>
        {
            await Task.Delay(1);
            return Result<string>.Ok($"Value: {x}");
        });

        // Assert
        bound.HasError.Should().BeTrue();
        bound.Error.Should().Be(error);
    }

    [Fact]
    public async Task BindAsync_WhenBindReturnsFailure_PropagatesNewError()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(5));
        var newError = new Error("NEW_ERR", "New error");

        // Act
        var bound = await resultTask.BindAsync(async _ =>
        {
            await Task.Delay(1);
            return Result<string>.Failed(newError);
        });

        // Assert
        bound.HasError.Should().BeTrue();
        bound.Error.Should().Be(newError);
    }

    [Fact]
    public async Task Bind_Sync_WithSuccessResult_ExecutesBindFunction()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(5));

        // Act
        var bound = await resultTask.Bind(x => Result<string>.Ok($"Value: {x}"));

        // Assert
        bound.Value.Should().Be("Value: 5");
    }

    #endregion

    #region TapAsync

    [Fact]
    public async Task TapAsync_WithSuccessResult_ExecutesSideEffect()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        var sideEffectValue = 0;

        // Act
        var result = await resultTask.TapAsync(async x =>
        {
            await Task.Delay(1);
            sideEffectValue = x;
        });

        // Assert
        sideEffectValue.Should().Be(42);
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task TapAsync_WithFailureResult_DoesNotExecuteSideEffect()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Failed(new Error("ERR", "Error")));
        var sideEffectExecuted = false;

        // Act
        await resultTask.TapAsync(async _ =>
        {
            await Task.Delay(1);
            sideEffectExecuted = true;
        });

        // Assert
        sideEffectExecuted.Should().BeFalse();
    }

    [Fact]
    public async Task Tap_Sync_WithSuccessResult_ExecutesSideEffect()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        var captured = 0;

        // Act
        await resultTask.Tap(x => captured = x);

        // Assert
        captured.Should().Be(42);
    }

    #endregion

    #region TapErrorAsync

    [Fact]
    public async Task TapErrorAsync_WithFailureResult_ExecutesSideEffect()
    {
        // Arrange
        var error = new Error("ERR", "Error message");
        var resultTask = Task.FromResult(Result<int>.Failed(error));
        string? capturedMessage = null;

        // Act
        await resultTask.TapErrorAsync(async e =>
        {
            await Task.Delay(1);
            capturedMessage = e.Message;
        });

        // Assert
        capturedMessage.Should().Be("Error message");
    }

    [Fact]
    public async Task TapErrorAsync_WithSuccessResult_DoesNotExecuteSideEffect()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        var sideEffectExecuted = false;

        // Act
        await resultTask.TapErrorAsync(async _ =>
        {
            await Task.Delay(1);
            sideEffectExecuted = true;
        });

        // Assert
        sideEffectExecuted.Should().BeFalse();
    }

    [Fact]
    public async Task TapError_Sync_WithFailureResult_ExecutesSideEffect()
    {
        // Arrange
        var error = new Error("ERR", "Error");
        var resultTask = Task.FromResult(Result<int>.Failed(error));
        Error? captured = null;

        // Act
        await resultTask.TapError(e => captured = e);

        // Assert
        captured.Should().Be(error);
    }

    #endregion

    #region FilterAsync

    [Fact]
    public async Task FilterAsync_WhenPredicateIsTrue_ReturnsOriginalResult()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        var error = new Error("FILTER", "Filter failed");

        // Act
        var filtered = await resultTask.FilterAsync(
            async x =>
            {
                await Task.Delay(1);
                return x > 0;
            },
            error
        );

        // Assert
        filtered.IsOk.Should().BeTrue();
        filtered.Value.Should().Be(42);
    }

    [Fact]
    public async Task FilterAsync_WhenPredicateIsFalse_ReturnsFailure()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(-5));
        var error = new Error("FILTER", "Value must be positive");

        // Act
        var filtered = await resultTask.FilterAsync(
            async x =>
            {
                await Task.Delay(1);
                return x > 0;
            },
            error
        );

        // Assert
        filtered.HasError.Should().BeTrue();
        filtered.Error.Should().Be(error);
    }

    [Fact]
    public async Task Filter_Sync_WhenPredicateIsTrue_ReturnsOriginalResult()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        var error = new Error("FILTER", "Filter failed");

        // Act
        var filtered = await resultTask.Filter(x => x > 0, error);

        // Assert
        filtered.IsOk.Should().BeTrue();
    }

    #endregion

    #region ThenAsync

    [Fact]
    public async Task ThenAsync_WithFunction_ExecutesFunction()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(5));

        // Act
        var chained = await resultTask.ThenAsync(async () =>
        {
            await Task.Delay(1);
            return Result<string>.Ok("Next");
        });

        // Assert
        chained.IsOk.Should().BeTrue();
        chained.Value.Should().Be("Next");
    }

    [Fact]
    public async Task ThenAsync_WithFailedFirst_DoesNotExecuteFunction()
    {
        // Arrange
        var error = new Error("ERR", "Error");
        var resultTask = Task.FromResult(Result<int>.Failed(error));
        var functionCalled = false;

        // Act
        var chained = await resultTask.ThenAsync(async () =>
        {
            await Task.Delay(1);
            functionCalled = true;
            return Result<string>.Ok("Next");
        });

        // Assert
        functionCalled.Should().BeFalse();
        chained.Error.Should().Be(error);
    }

    [Fact]
    public async Task ThenAsync_WithValueFunction_PassesValue()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(5));

        // Act
        var chained = await resultTask.ThenAsync(async x =>
        {
            await Task.Delay(1);
            return Result<string>.Ok($"Value: {x}");
        });

        // Assert
        chained.Value.Should().Be("Value: 5");
    }

    [Fact]
    public async Task Then_Sync_ChainsResults()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(5));
        var second = Result<string>.Ok("Second");

        // Act
        var chained = await resultTask.Then(second);

        // Assert
        chained.IsOk.Should().BeTrue();
        chained.Value.Should().Be("Second");
    }

    #endregion

    #region MatchAsync

    [Fact]
    public async Task MatchAsync_WithSuccessResult_ExecutesOnSuccess()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));

        // Act
        var output = await resultTask.MatchAsync(
            async value =>
            {
                await Task.Delay(1);
                return $"Value: {value}";
            },
            async error =>
            {
                await Task.Delay(1);
                return $"Error: {error.Message}";
            }
        );

        // Assert
        output.Should().Be("Value: 42");
    }

    [Fact]
    public async Task MatchAsync_WithFailureResult_ExecutesOnFailure()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Failed(new Error("ERR", "Something went wrong")));

        // Act
        var output = await resultTask.MatchAsync(
            async value =>
            {
                await Task.Delay(1);
                return $"Value: {value}";
            },
            async error =>
            {
                await Task.Delay(1);
                return $"Error: {error.Message}";
            }
        );

        // Assert
        output.Should().Be("Error: Something went wrong");
    }

    [Fact]
    public async Task Match_Sync_WithSuccessResult_ReturnsCorrectValue()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));

        // Act
        var output = await resultTask.Match(
            value => $"Value: {value}",
            error => $"Error: {error.Message}"
        );

        // Assert
        output.Should().Be("Value: 42");
    }

    #endregion

    #region Chaining Multiple Operations

    [Fact]
    public async Task ChainedOperations_WorkCorrectly()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(5));

        // Act
        var final = await resultTask
            .Map(x => x * 2)
            .Bind(x => Result<string>.Ok($"Value: {x}"))
            .Tap(s => { /* Side effect */ })
            .Map(s => s!.ToUpper());

        // Assert
        final.IsOk.Should().BeTrue();
        final.Value.Should().Be("VALUE: 10");
    }

    [Fact]
    public async Task ChainedOperations_ShortCircuitOnError()
    {
        // Arrange
        var error = new Error("ERR", "Initial error");
        var resultTask = Task.FromResult(Result<int>.Failed(error));
        var mapCalled = false;
        var bindCalled = false;

        // Act
        var final = await resultTask
            .Map(x =>
            {
                mapCalled = true;
                return x * 2;
            })
            .Bind(x =>
            {
                bindCalled = true;
                return Result<string>.Ok($"Value: {x}");
            });

        // Assert
        mapCalled.Should().BeFalse();
        bindCalled.Should().BeFalse();
        final.Error.Should().Be(error);
    }

    #endregion
}

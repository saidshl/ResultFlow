using ResultFlow.Errors;
using ResultFlow.Results;

namespace ResultFlow.Extensions;

/// <summary>
/// Provides extension methods for working with Task&lt;Result&lt;T&gt;&gt; to enable
/// fluent async operations with the Result pattern.
/// </summary>
public static class TaskResult
{
    /// <summary>
    /// Transforms the success value of a Task&lt;Result&lt;T&gt;&gt; using an async function.
    /// </summary>
    /// <typeparam name="TValue">The type of the current value.</typeparam>
    /// <typeparam name="TResult">The type of the new value.</typeparam>
    /// <param name="resultTask">The task containing the result.</param>
    /// <param name="map">Async function to transform the success value.</param>
    /// <returns>A new Task&lt;Result&lt;TResult&gt;&gt; with the transformed value, or the error if failed.</returns>
    public static async Task<Result<TResult>> MapAsync<TValue, TResult>(this Task<Result<TValue>> resultTask, Func<TValue?, Task<TResult>> map)
    {
        var result = await resultTask;
        if (result.HasError)
            return Result<TResult>.Failed(result.Error!);

        var mappedValue = await map(result.Value);
        return Result<TResult>.Ok(mappedValue);
    }

    /// <summary>
    /// Transforms the success value of a Task&lt;Result&lt;T&gt;&gt; using a synchronous function.
    /// </summary>
    /// <typeparam name="TValue">The type of the current value.</typeparam>
    /// <typeparam name="TResult">The type of the new value.</typeparam>
    /// <param name="resultTask">The task containing the result.</param>
    /// <param name="map">Function to transform the success value.</param>
    /// <returns>A new Task&lt;Result&lt;TResult&gt;&gt; with the transformed value, or the error if failed.</returns>
    public static async Task<Result<TResult>> Map<TValue, TResult>(this Task<Result<TValue>> resultTask, Func<TValue?, TResult> map)
    {
        var result = await resultTask;
        return result.Map(map);
    }

    /// <summary>
    /// Chains async operations on Task&lt;Result&lt;T&gt;&gt;. Flattens nested results.
    /// </summary>
    /// <typeparam name="TValue">The type of the current value.</typeparam>
    /// <typeparam name="TResult">The type of the new value.</typeparam>
    /// <param name="resultTask">The task containing the result.</param>
    /// <param name="bind">Async function that returns a new Result.</param>
    /// <returns>A new Task&lt;Result&lt;TResult&gt;&gt; from the bind function, or the error if failed.</returns>
    public static async Task<Result<TResult>> BindAsync<TValue, TResult>(this Task<Result<TValue>> resultTask, Func<TValue?, Task<Result<TResult>>> bind)
    {
        var result = await resultTask;
        if (result.HasError)
            return Result<TResult>.Failed(result.Error!);

        return await bind(result.Value);
    }

    /// <summary>
    /// Chains synchronous operations on Task&lt;Result&lt;T&gt;&gt;.
    /// </summary>
    /// <typeparam name="TValue">The type of the current value.</typeparam>
    /// <typeparam name="TResult">The type of the new value.</typeparam>
    /// <param name="resultTask">The task containing the result.</param>
    /// <param name="bind">Function that returns a new Result.</param>
    /// <returns>A new Task&lt;Result&lt;TResult&gt;&gt; from the bind function, or the error if failed.</returns>
    public static async Task<Result<TResult>> Bind<TValue, TResult>(this Task<Result<TValue>> resultTask, Func<TValue?, Result<TResult>> bind)
    {
        var result = await resultTask;
        return result.Bind(bind);
    }

    /// <summary>
    /// Executes an async side effect on the success value and returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="resultTask">The task containing the result.</param>
    /// <param name="action">Async side effect action to execute.</param>
    /// <returns>The original Task&lt;Result&lt;TValue&gt;&gt; instance.</returns>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Task<Result<TValue>> resultTask, Func<TValue?, Task> action)
    {
        var result = await resultTask;
        if (result.IsOk)
            await action(result.Value);
        return result;
    }

    /// <summary>
    /// Executes a synchronous side effect on the success value and returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="resultTask">The task containing the result.</param>
    /// <param name="action">Side effect action to execute.</param>
    /// <returns>The original Task&lt;Result&lt;TValue&gt;&gt; instance.</returns>
    public static async Task<Result<TValue>> Tap<TValue>(this Task<Result<TValue>> resultTask, Action<TValue?> action)
    {
        var result = await resultTask;
        return result.Tap(action);
    }

    /// <summary>
    /// Executes an async side effect on the error and returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="resultTask">The task containing the result.</param>
    /// <param name="action">Async side effect action to execute on the error.</param>
    /// <returns>The original Task&lt;Result&lt;TValue&gt;&gt; instance.</returns>
    public static async Task<Result<TValue>> TapErrorAsync<TValue>(this Task<Result<TValue>> resultTask, Func<Error, Task> action)
    {
        var result = await resultTask;
        if (result.HasError)
            await action(result.Error!);
        return result;
    }

    /// <summary>
    /// Executes a synchronous side effect on the error and returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="resultTask">The task containing the result.</param>
    /// <param name="action">Side effect action to execute on the error.</param>
    /// <returns>The original Task&lt;Result&lt;TValue&gt;&gt; instance.</returns>
    public static async Task<Result<TValue>> TapError<TValue>(this Task<Result<TValue>> resultTask, Action<Error> action)
    {
        var result = await resultTask;
        return result.TapError(action);
    }

    /// <summary>
    /// Filters the result based on an async predicate. Returns an error if the predicate is false.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="resultTask">The task containing the result.</param>
    /// <param name="predicate">Async predicate to test the value.</param>
    /// <param name="error">Error to return if the predicate is false.</param>
    /// <returns>The original result if the predicate is true, otherwise a failure result.</returns>
    public static async Task<Result<TValue>> FilterAsync<TValue>(this Task<Result<TValue>> resultTask, Func<TValue?, Task<bool>> predicate, Error error)
    {
        var result = await resultTask;
        if (result.HasError)
            return result;

        var predicateResult = await predicate(result.Value);
        return predicateResult ? result : Result<TValue>.Failed(error);
    }

    /// <summary>
    /// Filters the result based on a synchronous predicate.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="resultTask">The task containing the result.</param>
    /// <param name="predicate">Predicate to test the value.</param>
    /// <param name="error">Error to return if the predicate is false.</param>
    /// <returns>The original result if the predicate is true, otherwise a failure result.</returns>
    public static async Task<Result<TValue>> Filter<TValue>(this Task<Result<TValue>> resultTask, Func<TValue?, bool> predicate, Error error)
    {
        var result = await resultTask;
        return result.Filter(predicate, error);
    }

    /// <summary>
    /// Chains another async result. Returns the first failure encountered, otherwise returns the second result.
    /// </summary>
    /// <typeparam name="TValue">The type of the first result's value.</typeparam>
    /// <typeparam name="TResult">The type of the second result's value.</typeparam>
    /// <param name="resultTask">The task containing the first result.</param>
    /// <param name="other">Async function that returns the next result.</param>
    /// <returns>The first failure or the second result.</returns>
    public static async Task<Result<TResult>> ThenAsync<TValue, TResult>(this Task<Result<TValue>> resultTask, Func<Task<Result<TResult>>> other)
    {
        var result = await resultTask;
        if (result.HasError)
            return Result<TResult>.Failed(result.Error!);

        return await other();
    }

    /// <summary>
    /// Chains another async result that depends on the first result's value.
    /// </summary>
    /// <typeparam name="TValue">The type of the first result's value.</typeparam>
    /// <typeparam name="TResult">The type of the second result's value.</typeparam>
    /// <param name="resultTask">The task containing the first result.</param>
    /// <param name="then">Async function that returns the next result based on the current value.</param>
    /// <returns>The first failure or the result of the then function.</returns>
    public static Task<Result<TResult>> ThenAsync<TValue, TResult>(this Task<Result<TValue>> resultTask, Func<TValue?, Task<Result<TResult>>> then)
    {
        return resultTask.BindAsync(then);
    }

    /// <summary>
    /// Chains a synchronous result to an async result.
    /// </summary>
    /// <typeparam name="TValue">The type of the first result's value.</typeparam>
    /// <typeparam name="TResult">The type of the second result's value.</typeparam>
    /// <param name="resultTask">The task containing the first result.</param>
    /// <param name="other">The synchronous result to chain.</param>
    /// <returns>The first failure or the second result.</returns>
    public static async Task<Result<TResult>> Then<TValue, TResult>(this Task<Result<TValue>> resultTask, Result<TResult> other)
    {
        var result = await resultTask;
        return result.Then(other);
    }

    /// <summary>
    /// Performs an async pattern match on the result and returns a value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <typeparam name="TResult">The return type.</typeparam>
    /// <param name="resultTask">The task containing the result.</param>
    /// <param name="onSuccess">Async function to execute if the result is successful.</param>
    /// <param name="onFailure">Async function to execute if the result failed.</param>
    /// <returns>The result of the appropriate function.</returns>
    public static async Task<TResult> MatchAsync<TValue, TResult>(this Task<Result<TValue>> resultTask, Func<TValue?, Task<TResult>> onSuccess, Func<Error, Task<TResult>> onFailure)
    {
        var result = await resultTask;
        return result.IsOk
            ? await onSuccess(result.Value)
            : await onFailure(result.Error!);
    }

    /// <summary>
    /// Performs a pattern match on the result and returns a value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <typeparam name="TResult">The return type.</typeparam>
    /// <param name="resultTask">The task containing the result.</param>
    /// <param name="onSuccess">Function to execute if the result is successful.</param>
    /// <param name="onFailure">Function to execute if the result failed.</param>
    /// <returns>The result of the appropriate function.</returns>
    public static async Task<TResult> Match<TValue, TResult>(this Task<Result<TValue>> resultTask, Func<TValue?, TResult> onSuccess, Func<Error, TResult> onFailure)
    {
        var result = await resultTask;
        return result.Match(onSuccess, onFailure);
    }
}

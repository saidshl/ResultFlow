using ResultFlow.Errors;
using ResultFlow.Results;

namespace ResultFlow.Extensions;

/// <summary>
/// Provides extension methods for working with Task&lt;VoidResult&gt; to enable
/// fluent async operations with the void Result pattern, supporting chaining, side effects,
/// and pattern matching for operations that don't return values.
/// </summary>
public static class TaskVoidResult
{
    /// <summary>
    /// Executes an async side effect on a successful void result.
    /// </summary>
    /// <param name="resultTask">The task containing the result.</param>
    /// <param name="action">Async side effect action to execute.</param>
    /// <returns>The original Task&lt;Result&gt; instance.</returns>
    public static async Task<VoidResult> TapAsync(this Task<VoidResult> resultTask, Func<Task> action)
    {
        var result = await resultTask;
        if (result.IsOk)
            await action();
        return result;
    }

    /// <summary>
    /// Executes a synchronous side effect on a successful void result.
    /// </summary>
    /// <param name="resultTask">The task containing the result.</param>
    /// <param name="action">Side effect action to execute.</param>
    /// <returns>The original Task&lt;Result&gt; instance.</returns>
    public static async Task<VoidResult> Tap(this Task<VoidResult> resultTask, Action action)
    {
        var result = await resultTask;
        return result.Tap(action);
    }

    /// <summary>
    /// Executes an async side effect on the error of a void result.
    /// </summary>
    /// <param name="resultTask">The task containing the result.</param>
    /// <param name="action">Async side effect action to execute on the error.</param>
    /// <returns>The original Task&lt;Result&gt; instance.</returns>
    public static async Task<VoidResult> TapErrorAsync(this Task<VoidResult> resultTask, Func<Error, Task> action)
    {
        var result = await resultTask;
        if (result.HasError)
            await action(result.Error!);
        return result;
    }

    /// <summary>
    /// Executes a synchronous side effect on the error of a void result.
    /// </summary>
    /// <param name="resultTask">The task containing the result.</param>
    /// <param name="action">Side effect action to execute on the error.</param>
    /// <returns>The original Task&lt;Result&gt; instance.</returns>
    public static async Task<VoidResult> TapError(this Task<VoidResult> resultTask, Action<Error> action)
    {
        var result = await resultTask;
        return result.TapError(action);
    }

    /// <summary>
    /// Chains another async void result.
    /// </summary>
    /// <param name="resultTask">The task containing the first result.</param>
    /// <param name="other">Async function that returns the next result.</param>
    /// <returns>The first failure or the second result.</returns>
    public static async Task<VoidResult> ThenAsync(this Task<VoidResult> resultTask, Func<Task<VoidResult>> other)
    {
        var result = await resultTask;
        if (result.HasError)
            return result;

        return await other();
    }

    /// <summary>
    /// Chains a synchronous void result to an async void result.
    /// </summary>
    /// <param name="resultTask">The task containing the first result.</param>
    /// <param name="other">The synchronous result to chain.</param>
    /// <returns>The first failure or the second result.</returns>
    public static async Task<VoidResult> Then(this Task<VoidResult> resultTask, VoidResult other)
    {
        var result = await resultTask;
        return result.Then(other);
    }

    /// <summary>
    /// Chains an async generic result to a void result.
    /// </summary>
    /// <typeparam name="TValue">The type of the second result's value.</typeparam>
    /// <param name="resultTask">The task containing the void result.</param>
    /// <param name="other">Async function that returns the next result.</param>
    /// <returns>The first failure or the second result.</returns>
    public static async Task<Result<TValue>> ThenAsync<TValue>(this Task<VoidResult> resultTask, Func<Task<Result<TValue>>> other)
    {
        var result = await resultTask;
        if (result.HasError)
            return Result<TValue>.Failed(result.Error!);

        return await other();
    }

    /// <summary>
    /// Chains a synchronous generic result to an async void result.
    /// </summary>
    /// <typeparam name="TValue">The type of the second result's value.</typeparam>
    /// <param name="resultTask">The task containing the void result.</param>
    /// <param name="other">The synchronous result to chain.</param>
    /// <returns>The first failure or the second result.</returns>
    public static async Task<Result<TValue>> Then<TValue>(this Task<VoidResult> resultTask, Result<TValue> other)
    {
        var result = await resultTask;
        return result.Then(other);
    }

    /// <summary>
    /// Performs an async pattern match on a void result and returns a value.
    /// </summary>
    /// <typeparam name="TResult">The return type.</typeparam>
    /// <param name="resultTask">The task containing the result.</param>
    /// <param name="onSuccess">Async function to execute if successful.</param>
    /// <param name="onFailure">Async function to execute if failed.</param>
    /// <returns>The result of the appropriate function.</returns>
    public static async Task<TResult> MatchAsync<TResult>(this Task<VoidResult> resultTask, Func<Task<TResult>> onSuccess, Func<Error, Task<TResult>> onFailure)
    {
        var result = await resultTask;
        return result.IsOk
            ? await onSuccess()
            : await onFailure(result.Error!);
    }

    /// <summary>
    /// Performs a pattern match on a void result and returns a value.
    /// </summary>
    /// <typeparam name="TResult">The return type.</typeparam>
    /// <param name="resultTask">The task containing the result.</param>
    /// <param name="onSuccess">Function to execute if successful.</param>
    /// <param name="onFailure">Function to execute if failed.</param>
    /// <returns>The result of the appropriate function.</returns>
    public static async Task<TResult> Match<TResult>(this Task<VoidResult> resultTask, Func<TResult> onSuccess, Func<Error, TResult> onFailure)
    {
        var result = await resultTask;
        return result.Match(onSuccess, onFailure);
    }
}

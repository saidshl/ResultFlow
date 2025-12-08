using ResultFlow.Errors;

namespace ResultFlow.Results;

/// <summary>
/// Provides factory methods for creating void Result instances (operations that don't return a value).
/// This is a convenience wrapper around VoidResult for a cleaner API.
/// </summary>
public static class Result
{
    /// <summary>
    /// Creates a successful void result.
    /// </summary>
    /// <returns>A successful VoidResult instance.</returns>
    public static VoidResult Success() => VoidResult.Ok();

    /// <summary>
    /// Creates a successful void result (alias for Success).
    /// </summary>
    /// <returns>A successful VoidResult instance.</returns>
    public static VoidResult Ok() => VoidResult.Ok();

    /// <summary>
    /// Creates a failed void result with the given error.
    /// </summary>
    /// <param name="error">The error that occurred.</param>
    /// <returns>A failed VoidResult instance.</returns>
    public static VoidResult Failure(Error error) => VoidResult.Failed(error);

    /// <summary>
    /// Creates a failed void result with the given error (alias for Failure).
    /// </summary>
    /// <param name="error">The error that occurred.</param>
    /// <returns>A failed VoidResult instance.</returns>
    public static VoidResult Failed(Error error) => VoidResult.Failed(error);

    /// <summary>
    /// Creates a Result from an operation that might throw an exception.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="errorFactory">Optional factory to create error from exception.</param>
    /// <returns>A Result containing the value or the error.</returns>
    public static Result<TValue> Try<TValue>(Func<TValue> operation, Func<Exception, Error>? errorFactory = null)
    {
        try
        {
            return Result<TValue>.Ok(operation());
        }
        catch (Exception ex)
        {
            var error = errorFactory?.Invoke(ex) ?? InternalServerError.FromException(ex);
            return Result<TValue>.Failed(error);
        }
    }

    /// <summary>
    /// Creates a Result from an async operation that might throw an exception.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="operation">The async operation to execute.</param>
    /// <param name="errorFactory">Optional factory to create error from exception.</param>
    /// <returns>A Task containing the Result with the value or the error.</returns>
    public static async Task<Result<TValue>> TryAsync<TValue>(Func<Task<TValue>> operation, Func<Exception, Error>? errorFactory = null)
    {
        try
        {
            var value = await operation();
            return Result<TValue>.Ok(value);
        }
        catch (Exception ex)
        {
            var error = errorFactory?.Invoke(ex) ?? InternalServerError.FromException(ex);
            return Result<TValue>.Failed(error);
        }
    }

    /// <summary>
    /// Creates a void Result from an operation that might throw an exception.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="errorFactory">Optional factory to create error from exception.</param>
    /// <returns>A VoidResult indicating success or failure.</returns>
    public static VoidResult Try(Action operation, Func<Exception, Error>? errorFactory = null)
    {
        try
        {
            operation();
            return VoidResult.Ok();
        }
        catch (Exception ex)
        {
            var error = errorFactory?.Invoke(ex) ?? InternalServerError.FromException(ex);
            return VoidResult.Failed(error);
        }
    }

    /// <summary>
    /// Creates a void Result from an async operation that might throw an exception.
    /// </summary>
    /// <param name="operation">The async operation to execute.</param>
    /// <param name="errorFactory">Optional factory to create error from exception.</param>
    /// <returns>A Task containing the VoidResult indicating success or failure.</returns>
    public static async Task<VoidResult> TryAsync(Func<Task> operation, Func<Exception, Error>? errorFactory = null)
    {
        try
        {
            await operation();
            return VoidResult.Ok();
        }
        catch (Exception ex)
        {
            var error = errorFactory?.Invoke(ex) ?? InternalServerError.FromException(ex);
            return VoidResult.Failed(error);
        }
    }

    /// <summary>
    /// Combines multiple results into a single result. Returns the first failure or success with all values.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="results">The results to combine.</param>
    /// <returns>A Result containing all values or the first error.</returns>
    public static Result<IReadOnlyList<T>> Combine<T>(params Result<T>[] results)
    {
        var values = new List<T>();
        foreach (var result in results)
        {
            if (result.HasError)
                return Result<IReadOnlyList<T>>.Failed(result.Error!);
            values.Add(result.Value!);
        }
        return Result<IReadOnlyList<T>>.Ok(values);
    }

    /// <summary>
    /// Combines multiple results into a single result. Returns the first failure or success with all values.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="results">The results to combine.</param>
    /// <returns>A Result containing all values or the first error.</returns>
    public static Result<IReadOnlyList<T>> Combine<T>(IEnumerable<Result<T>> results)
    {
        var values = new List<T>();
        foreach (var result in results)
        {
            if (result.HasError)
                return Result<IReadOnlyList<T>>.Failed(result.Error!);
            values.Add(result.Value!);
        }
        return Result<IReadOnlyList<T>>.Ok(values);
    }
}

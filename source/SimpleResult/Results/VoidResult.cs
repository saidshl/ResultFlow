using SimpleResult.Errors;

namespace SimpleResult.Results;

/// <summary>
/// Represents the outcome of an operation that doesn't return a value,
/// only indicating success or failure.
/// </summary>
public readonly struct Result : IEquatable<Result>
{
    private readonly Error? _error;

    /// <summary>
    /// Gets the error. Only valid when IsFailure is true.
    /// </summary>
    public Error? Error => _error;

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsOk { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool HasError => !IsOk;

    /// <summary>
    /// Private constructor for success result.
    /// </summary>
    public Result()
    {
        _error = null;
        IsOk = true;
    }

    /// <summary>
    /// Private constructor for failure result.
    /// </summary>
    private Result(Error error)
    {
        _error = error ?? throw new ArgumentNullException(nameof(error));
        IsOk = false;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful Result instance.</returns>
    public static Result Success() => new();

    /// <summary>
    /// Creates a failed result with the given error.
    /// </summary>
    /// <param name="error">The error that occurred.</param>
    /// <returns>A failed Result instance.</returns>
    public static Result Failure(Error error) => new(error);

    /// <summary>
    /// Performs a pattern match on the result.
    /// </summary>
    /// <param name="onSuccess">Action to execute if successful.</param>
    /// <param name="onFailure">Action to execute if failed.</param>
    public void Match(Action onSuccess, Action<Error> onFailure)
    {
        if (IsOk)
            onSuccess();
        else
            onFailure(_error!);
    }

    /// <summary>
    /// Performs a pattern match and returns a value.
    /// </summary>
    public TResult Match<TResult>(Func<TResult> onSuccess, Func<Error, TResult> onFailure)
        => IsOk ? onSuccess() : onFailure(_error!);

    /// <summary>
    /// Executes a side effect action.
    /// </summary>
    public Result Tap(Action action)
    {
        if (IsOk)
            action();
        return this;
    }

    /// <summary>
    /// Executes a side effect on error.
    /// </summary>
    public Result TapError(Action<Error> action)
    {
        if (HasError)
            action(_error!);
        return this;
    }

    /// <summary>
    /// Gets the error or throws if successful.
    /// </summary>
    public Error? GetErrorOrThrow() =>
        HasError ? _error : throw new InvalidOperationException("Operation succeeded");

    /// <summary>
    /// Gets a string representation of the result.
    /// </summary>
    public override string ToString() =>
        IsOk ? "Success()" : $"Failure({_error?.Code}: {_error?.Message})";

    /// <summary>
    /// Compares two Result instances for equality.
    /// </summary>
    public bool Equals(Result other)
    {
        if (IsOk != other.IsOk)
            return false;

        if (IsOk)
            return true;

        return _error?.Code == other._error?.Code && _error?.Message == other._error?.Message;
    }

    /// <summary>
    /// Compares this result with another object for equality.
    /// </summary>
    public override bool Equals(object? obj) =>
        obj is Result other && Equals(other);

    /// <summary>
    /// Gets the hash code of the result.
    /// </summary>
    public override int GetHashCode() =>
        IsOk ? HashCode.Combine(IsOk) : HashCode.Combine(IsOk, _error?.Code);

    /// <summary>
    /// Implicit conversion from an Error to a failed Result.
    /// </summary>
    public static implicit operator Result(Error error) => Failure(error);

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(Result left, Result right) => left.Equals(right);

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(Result left, Result right) => !left.Equals(right);
}

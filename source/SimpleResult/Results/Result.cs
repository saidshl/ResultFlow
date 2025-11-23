using SimpleResult.Errors;

namespace SimpleResult.Results;

/// <summary>
/// Represents the outcome of an operation that can either succeed with a value or fail with an error.
/// A strongly-typed, immutable result pattern implementation.
/// </summary>
/// <typeparam name="TValue">The type of the success value.</typeparam>
public readonly struct Result<TValue> : IEquatable<Result<TValue>>
{
    private readonly TValue? _value;
    private readonly Error? _error;

    /// <summary>
    /// Gets the success value. Only valid when IsSuccess is true.
    /// </summary>
    public TValue? Value => _value;

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
    private Result(TValue value)
    {
        _value = value;
        _error = null;
        IsOk = true;
    }

    /// <summary>
    /// Private constructor for failure result.
    /// </summary>
    private Result(Error error)
    {
        _error = error ?? throw new ArgumentNullException(nameof(error));
        _value = default;
        IsOk = false;
    }

    /// <summary>
    /// Creates a successful result with the given value.
    /// </summary>
    /// <param name="value">The success value.</param>
    /// <returns>A successful Result instance.</returns>
    public static Result<TValue> Success(TValue value) => new(value);

    /// <summary>
    /// Creates a failed result with the given error.
    /// </summary>
    /// <param name="error">The error that occurred.</param>
    /// <returns>A failed Result instance.</returns>
    public static Result<TValue> Failure(Error error) => new(error);

    /// <summary>
    /// Performs a pattern match on the result and returns a value.
    /// </summary>
    /// <typeparam name="TResult">The return type.</typeparam>
    /// <param name="onSuccess">Function to execute if the result is successful.</param>
    /// <param name="onFailure">Function to execute if the result failed.</param>
    /// <returns>The result of the appropriate function.</returns>
    public TResult Match<TResult>(Func<TValue?, TResult> onSuccess, Func<Error, TResult> onFailure)
        => IsOk ? onSuccess(_value) : onFailure(_error!);

    /// <summary>
    /// Performs a pattern match on the result and executes an action.
    /// </summary>
    /// <param name="onSuccess">Action to execute if the result is successful.</param>
    /// <param name="onFailure">Action to execute if the result failed.</param>
    public void Match(Action<TValue?> onSuccess, Action<Error> onFailure)
    {
        if (IsOk)
            onSuccess(_value);
        else
            onFailure(_error!);
    }

    /// <summary>
    /// Transforms the success value using the given function. Returns the error unchanged if failed.
    /// </summary>
    /// <typeparam name="TResult">The type of the new value.</typeparam>
    /// <param name="map">Function to transform the success value.</param>
    /// <returns>A new Result with the transformed value, or the error if failed.</returns>
    public Result<TResult> Map<TResult>(Func<TValue?, TResult> map)
        => IsOk ? Result<TResult>.Success(map(_value)) : Result<TResult>.Failure(_error!);

    /// <summary>
    /// Chains operations on the result. Flattens nested results.
    /// </summary>
    /// <typeparam name="TResult">The type of the new value.</typeparam>
    /// <param name="bind">Function that returns a new Result.</param>
    /// <returns>A new Result from the bind function, or the error if failed.</returns>
    public Result<TResult> Bind<TResult>(Func<TValue?, Result<TResult>> bind)
        => IsOk ? bind(_value) : Result<TResult>.Failure(_error!);

    /// <summary>
    /// Executes a side effect function and returns the original result unchanged.
    /// </summary>
    /// <param name="action">Side effect action to execute on the success value.</param>
    /// <returns>The original Result instance.</returns>
    public Result<TValue> Tap(Action<TValue?> action)
    {
        if (IsOk)
            action(_value);
        return this;
    }

    /// <summary>
    /// Executes a side effect function on the error and returns the original result unchanged.
    /// </summary>
    /// <param name="action">Side effect action to execute on the error.</param>
    /// <returns>The original Result instance.</returns>
    public Result<TValue> TapError(Action<Error> action)
    {
        if (HasError)
            action(_error!);
        return this;
    }

    /// <summary>
    /// Gets the success value or returns a default value if the result failed.
    /// </summary>
    /// <param name="defaultValue">The default value to return on failure.</param>
    /// <returns>The success value or the default value.</returns>
    public TValue? GetValueOrDefault(TValue? defaultValue = default) =>
        IsOk ? _value : defaultValue;

    /// <summary>
    /// Gets the success value or executes a function to get a fallback value on failure.
    /// </summary>
    /// <param name="onFailure">Function to execute if the result failed.</param>
    /// <returns>The success value or the result of the fallback function.</returns>
    public TValue? GetValueOrElse(Func<Error, TValue?> onFailure) =>
        IsOk ? _value : onFailure(_error!);

    /// <summary>
    /// Throws an exception if the result failed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the result failed.</exception>
    /// <returns>The success value.</returns>
    public TValue? GetValueOrThrow() =>
        IsOk ? _value : throw new InvalidOperationException($"Operation failed: {_error?.Message}");

    /// <summary>
    /// Throws a custom exception if the result failed.
    /// </summary>
    /// <param name="exceptionFactory">Function to create the exception from the error.</param>
    /// <returns>The success value.</returns>
    public TValue? GetValueOrThrow(Func<Error, Exception> exceptionFactory) =>
        IsOk ? _value : throw exceptionFactory(_error!);

    /// <summary>
    /// Filters the result based on a predicate. Returns an error if the predicate is false.
    /// </summary>
    /// <param name="predicate">Predicate to test the value.</param>
    /// <param name="error">Error to return if the predicate is false.</param>
    /// <returns>The original result if the predicate is true, otherwise a failure result.</returns>
    public Result<TValue> Filter(Func<TValue?, bool> predicate, Error error)
    {
        if (HasError)
            return this;
        return predicate(_value) ? this : Failure(error);
    }

    /// <summary>
    /// Chains another result. Returns the first failure encountered, otherwise returns the second result.
    /// </summary>
    /// <typeparam name="TResult">The type of the second result's value.</typeparam>
    /// <param name="other">The other result to chain.</param>
    /// <returns>The first failure or the second result.</returns>
    public Result<TResult> Then<TResult>(Result<TResult> other) =>
        HasError ? Result<TResult>.Failure(_error!) : other;

    /// <summary>
    /// Chains another result that depends on this result's value.
    /// </summary>
    /// <typeparam name="TResult">The type of the second result's value.</typeparam>
    /// <param name="then">Function that returns the next result based on the current value.</param>
    /// <returns>The first failure or the result of the then function.</returns>
    public Result<TResult> Then<TResult>(Func<TValue?, Result<TResult>> then) =>
        Bind(then);

    /// <summary>
    /// Gets a string representation of the result.
    /// </summary>
    /// <returns>A string describing whether the result succeeded or failed.</returns>
    public override string ToString() =>
        IsOk
            ? $"Success({_value})"
            : $"Failure({_error?.Code}: {_error?.Message})";

    /// <summary>
    /// Compares two Result instances for equality.
    /// </summary>
    /// <param name="other">The other Result to compare.</param>
    /// <returns>True if both results are equal, false otherwise.</returns>
    public bool Equals(Result<TValue> other)
    {
        if (IsOk != other.IsOk)
            return false;

        if (IsOk)
            return EqualityComparer<TValue>.Default.Equals(_value, other._value);

        return _error?.Code == other._error?.Code && _error?.Message == other._error?.Message;
    }

    /// <summary>
    /// Compares this result with another object for equality.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns>True if the objects are equal, false otherwise.</returns>
    public override bool Equals(object? obj) =>
        obj is Result<TValue> other && Equals(other);

    /// <summary>
    /// Gets the hash code of the result.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() =>
        IsOk
            ? HashCode.Combine(IsOk, _value)
            : HashCode.Combine(IsOk, _error?.Code, _error?.Message);

    /// <summary>
    /// Implicit conversion from a success value to a Result.
    /// </summary>
    /// <param name="value">The success value.</param>
    public static implicit operator Result<TValue>(TValue value) => Success(value);

    /// <summary>
    /// Implicit conversion from an Error to a failed Result.
    /// </summary>
    /// <param name="error">The error.</param>
    public static implicit operator Result<TValue>(Error error) => Failure(error);

    /// <summary>
    /// Equality operator for Result instances.
    /// </summary>
    public static bool operator ==(Result<TValue> left, Result<TValue> right) =>
        left.Equals(right);

    /// <summary>
    /// Inequality operator for Result instances.
    /// </summary>
    public static bool operator !=(Result<TValue> left, Result<TValue> right) =>
        !left.Equals(right);
}
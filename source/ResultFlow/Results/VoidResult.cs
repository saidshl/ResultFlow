using ResultFlow.Errors;

namespace ResultFlow.Results;

/// <summary>
/// Represents the outcome of an operation that doesn't return a value,
/// only indicating success or failure.
/// </summary>
public readonly struct VoidResult : IEquatable<VoidResult>
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
    public VoidResult()
    {
        _error = null;
        IsOk = true;
    }

    /// <summary>
    /// Private constructor for failure result.
    /// </summary>
    private VoidResult(Error error)
    {
        _error = error ?? throw new ArgumentNullException(nameof(error));
        IsOk = false;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful Result instance.</returns>
    public static VoidResult Ok() => new();

    /// <summary>
    /// Creates a failed result with the given error.
    /// </summary>
    /// <param name="error">The error that occurred.</param>
    /// <returns>A failed Result instance.</returns>
    public static VoidResult Failed(Error error) => new(error);

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
    /// <typeparam name="TResult">The return type.</typeparam>
    /// <param name="onSuccess">Function to execute if successful.</param>
    /// <param name="onFailure">Function to execute if failed.</param>
    /// <returns>The result of the appropriate function.</returns>
    public TResult Match<TResult>(Func<TResult> onSuccess, Func<Error, TResult> onFailure)
        => IsOk ? onSuccess() : onFailure(_error!);

    /// <summary>
    /// Executes a side effect action and returns the original result unchanged.
    /// </summary>
    /// <param name="action">Side effect action to execute if successful.</param>
    /// <returns>The original Result instance.</returns>
    public VoidResult Tap(Action action)
    {
        if (IsOk)
            action();
        return this;
    }

    /// <summary>
    /// Executes a side effect action on the error and returns the original result unchanged.
    /// </summary>
    /// <param name="action">Side effect action to execute on the error.</param>
    /// <returns>The original Result instance.</returns>
    public VoidResult TapError(Action<Error> action)
    {
        if (HasError)
            action(_error!);
        return this;
    }

    /// <summary>
    /// Chains another result. Returns the first failure encountered, otherwise returns the second result.
    /// </summary>
    /// <param name="other">The other result to chain.</param>
    /// <returns>The first failure or the second result.</returns>
    public VoidResult Then(VoidResult other) =>
        HasError ? this : other;

    /// <summary>
    /// Chains another result that depends on the success of this result.
    /// </summary>
    /// <param name="then">Function that returns the next result.</param>
    /// <returns>The first failure or the result of the then function.</returns>
    public VoidResult Then(Func<VoidResult> then) =>
        IsOk ? then() : this;

    /// <summary>
    /// Chains a generic result. Returns the first failure encountered, otherwise returns the second result.
    /// </summary>
    /// <typeparam name="TValue">The type of the second result's value.</typeparam>
    /// <param name="other">The other result to chain.</param>
    /// <returns>The first failure or the second result.</returns>
    public Result<TValue> Then<TValue>(Result<TValue> other) =>
        HasError ? Result<TValue>.Failed(_error!) : other;

    /// <summary>
    /// Chains a generic result that depends on the success of this result.
    /// </summary>
    /// <typeparam name="TValue">The type of the second result's value.</typeparam>
    /// <param name="then">Function that returns the next result.</param>
    /// <returns>The first failure or the result of the then function.</returns>
    public Result<TValue> Then<TValue>(Func<Result<TValue>> then) =>
        IsOk ? then() : Result<TValue>.Failed(_error!);

    /// <summary>
    /// Gets the error or throws if successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the operation succeeded.</exception>
    /// <returns>The error.</returns>
    public Error? GetErrorOrThrow() =>
        HasError ? _error : throw new InvalidOperationException("Operation succeeded");

    /// <summary>
    /// Gets a string representation of the result.
    /// </summary>
    /// <returns>A string describing whether the result succeeded or failed.</returns>
    public override string ToString() =>
        IsOk ? "Success()" : $"Failure({_error?.Code}: {_error?.Message})";

    /// <summary>
    /// Compares two Result instances for equality.
    /// </summary>
    /// <param name="other">The other Result to compare.</param>
    /// <returns>True if both results are equal, false otherwise.</returns>
    public bool Equals(VoidResult other)
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
    /// <param name="obj">The object to compare.</param>
    /// <returns>True if the objects are equal, false otherwise.</returns>
    public override bool Equals(object? obj) =>
        obj is VoidResult other && Equals(other);

    /// <summary>
    /// Gets the hash code of the result.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() =>
        IsOk ? HashCode.Combine(IsOk) : HashCode.Combine(IsOk, _error?.Code, _error?.Message);

    /// <summary>
    /// Implicit conversion from an Error to a failed Result.
    /// </summary>
    /// <param name="error">The error.</param>
    public static implicit operator VoidResult(Error error) => Failed(error);

    /// <summary>
    /// Equality operator for Result instances.
    /// </summary>
    public static bool operator ==(VoidResult left, VoidResult right) => left.Equals(right);

    /// <summary>
    /// Inequality operator for Result instances.
    /// </summary>
    public static bool operator !=(VoidResult left, VoidResult right) => !left.Equals(right);
}

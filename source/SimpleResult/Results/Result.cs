using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SimpleResult.Results;

public struct Result<TValue>
{
    public TValue? Value;
    public Error? Error;
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    private Result(TValue value)
    {
        Value = value;
        IsSuccess = true;
        Error = null;
    }
    private Result(Error error)
    {
        Error = error;
        IsSuccess = false;
        Value = default;
    }

    public static Result<TValue> Success(TValue value) => new(value);
    public static Result<TValue> Failure(Error error) => new(error);
}

using FluentValidation;
using FluentValidation.Results;
using SimpleResult.Errors;
using SimpleResult.Results;
using System.Text.Json;

namespace SimpleResult.FluentValidation.Extensions;

/// <summary>
/// Extension methods for converting FluentValidation results to SimpleResult.Results.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts FluentValidation results to Result{T}
    /// </summary>
    /// <typeparam name="T">The type of the value being validated.</typeparam>
    /// <param name="validationResult">The FluentValidation result to convert.</param>
    /// <param name="value">The value associated with the validation result.</param>
    /// <returns>A successful Result{T} if validation passed, otherwise a failed Result{T} with validation errors.</returns>
    public static Result<T> ToResult<T>(this ValidationResult validationResult, T? value = default)
    {
        if (validationResult.IsValid)
            return Result<T>.Success(value!);

        var errors = validationResult.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                x => x.Key,
                x => x.Select(e => e.ErrorMessage).ToList());

        return Result<T>.Failure(
            ValidationError.WithDefaults(
                "Validation failed",
                JsonSerializer.Serialize(errors),
                new Dictionary<string, object> { { "errors", errors } }
            )
        );
    }

    /// <summary>
    /// Validates an object and returns Result{T}
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="validator">The FluentValidation validator to use.</param>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>A successful Result{T} if validation passed, otherwise a failed Result{T} with validation errors.</returns>
    public static async Task<Result<T>> ValidateAsync<T>(this IValidator<T> validator, T instance)
    {
        var result = await validator.ValidateAsync(instance);
        return result.ToResult(instance);
    }
}

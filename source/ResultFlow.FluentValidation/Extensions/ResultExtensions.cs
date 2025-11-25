using FluentValidation;
using FluentValidation.Results;
using ResultFlow.Errors;
using ResultFlow.Results;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ResultFlow.FluentValidation.Extensions;

/// <summary>
/// Extension methods for integrating FluentValidation with ResultFlow.
/// Provides seamless conversion between FluentValidation results and the Result pattern.
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
            return Result<T>.Ok(value!);

        var errors = validationResult.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                x => x.Key,
                x => x.Select(e => e.ErrorMessage).ToList());

        return Result<T>.Failed(
            ValidationError.WithDefaults(
                "Validation failed",
                JsonSerializer.Serialize(errors, ValidationErrorJsonContext.Default.DictionaryStringListString),
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

/// <summary>
/// JSON serialization context for validation errors to support trimming and Native AOT.
/// </summary>
[JsonSerializable(typeof(Dictionary<string, List<string>>))]
internal partial class ValidationErrorJsonContext : JsonSerializerContext
{
}

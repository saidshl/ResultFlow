using Microsoft.AspNetCore.Mvc;
using ResultFlow.Errors;
using ResultFlow.Results;

namespace ResultFlow.Extensions.AspNetCore;

/// <summary>
/// Extension methods for converting Result and Result{TValue} to IActionResult for ASP.NET Core controllers.
/// Automatically maps error types to appropriate HTTP status codes.
/// </summary>
public static class AspNetMvcExtensions
{
    /// <summary>
    /// Converts a Result{TValue} to an IActionResult.
    /// Returns 200 OK with the value on success, or appropriate HTTP status code based on error type on failure.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>An IActionResult representing the result state.</returns>
    public static IActionResult ToActionResult<TValue>(this Result<TValue> result)
    {
        return result.IsOk
            ? (IActionResult)new OkObjectResult(result.Value)
            : ErrorToActionResult(result.Error!);
    }

    /// <summary>
    /// Converts a Task&lt;Result&lt;TValue&gt;&gt; to an IActionResult.
    /// Awaits the task and returns 200 OK with the value on success, or appropriate HTTP status code based on error type on failure.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="resultTask">The task containing the result to convert.</param>
    /// <returns>A task that represents the asynchronous operation and contains an IActionResult representing the result state.</returns>
    public static async Task<IActionResult> ToActionResultAsync<TValue>(this Task<Result<TValue>> resultTask)
    {
        var result = await resultTask;
        return result.ToActionResult();
    }

    /// <summary>
    /// Converts a Result to an IActionResult (for void operations).
    /// Returns 200 OK on success, or appropriate HTTP status code based on error type on failure.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <returns>An IActionResult representing the result state.</returns>
    public static IActionResult ToActionResult(this VoidResult result)
    {
        return result.IsOk
            ? (IActionResult)new OkResult()
            : ErrorToActionResult(result.Error!);
    }

    /// <summary>
    /// Converts a Task&lt;VoidResult&gt; to an IActionResult (for async void operations).
    /// Awaits the task and returns 200 OK on success, or appropriate HTTP status code based on error type on failure.
    /// </summary>
    /// <param name="resultTask">The task containing the result to convert.</param>
    /// <returns>A task that represents the asynchronous operation and contains an IActionResult representing the result state.</returns>
    public static async Task<IActionResult> ToActionResultAsync(this Task<VoidResult> resultTask)
    {
        var result = await resultTask;
        return result.ToActionResult();
    }

    /// <summary>
    /// Converts an Error to the appropriate HTTP status code and IActionResult based on error type.
    /// </summary>
    private static IActionResult ErrorToActionResult(Error error)
    {
        return error switch
        {
            // 400 - Bad Request
            BadRequestError badRequest =>
                new BadRequestObjectResult(new
                {
                    code = badRequest.Code,
                    message = badRequest.Message,
                    details = badRequest.Details,
                    metadata = badRequest.Metadata,
                    timestamp = DateTime.UtcNow
                }),

            // 401 - Unauthorized
            UnauthorizedError unauthorized =>
                new ObjectResult(new
                {
                    code = unauthorized.Code,
                    message = unauthorized.Message,
                    details = unauthorized.Details,
                    metadata = unauthorized.Metadata,
                    timestamp = DateTime.UtcNow
                })
                {
                    StatusCode = 401
                },

            // validation Error could also map to 422 - Unprocessable Entity
            ValidationError validation =>
                new ObjectResult(new
                {
                    code = validation.Code,
                    message = validation.Message,
                    details = validation.Details,
                    metadata = validation.Metadata,
                    timestamp = DateTime.UtcNow
                })
                {
                    StatusCode = 422
                },

            // 403 - Forbidden
            ForbiddenError forbidden =>
                new ObjectResult(new
                {
                    code = forbidden.Code,
                    message = forbidden.Message,
                    details = forbidden.Details,
                    metadata = forbidden.Metadata,
                    timestamp = DateTime.UtcNow
                })
                {
                    StatusCode = 403
                },

            // 404 - Not Found
            NotFoundError notFound =>
                new NotFoundObjectResult(new
                {
                    code = notFound.Code,
                    message = notFound.Message,
                    details = notFound.Details,
                    metadata = notFound.Metadata,
                    timestamp = DateTime.UtcNow
                }),

            // 409 - Conflict
            ConflictError conflict =>
                new ConflictObjectResult(new
                {
                    code = conflict.Code,
                    message = conflict.Message,
                    details = conflict.Details,
                    metadata = conflict.Metadata,
                    timestamp = DateTime.UtcNow
                }),

            // 429 - Too Many Requests
            TooManyRequestsError tooMany =>
                new ObjectResult(new
                {
                    code = tooMany.Code,
                    message = tooMany.Message,
                    details = tooMany.Details,
                    metadata = tooMany.Metadata,
                    timestamp = DateTime.UtcNow
                })
                {
                    StatusCode = 429
                },
            // 500 - Internal Server Error
            InternalServerError internalError =>
                new ObjectResult(new
                {
                    code = internalError.Code,
                    message = internalError.Message,
                    details = internalError.Details,
                    metadata = internalError.Metadata,
                    timestamp = DateTime.UtcNow
                })
                {
                    StatusCode = 500
                },

            _ =>
                new BadRequestObjectResult(new
                {
                    code = error.Code,
                    message = error.Message,
                    details = error.Details,
                    metadata = error.Metadata,
                    timestamp = DateTime.UtcNow
                })
        };
    }
}

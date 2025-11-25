# ResultFlow.AspNetCore

ASP.NET Core integration for [ResultFlow](https://github.com/said1993/ResultFlow).

This package provides extension methods to seamlessly convert `Result<TValue>` and `Result` objects into ASP.NET Core `IActionResult` responses with automatic HTTP status code mapping.

## ?? Installation

Install the NuGet package:

```bash
dotnet add package Shl.ResultFlow.AspNetCore --version 1.0.1-beta
```

Or via Package Manager:

```powershell
Install-Package Shl.ResultFlow.AspNetCore -Version 1.0.1-beta
```

## ?? Quick Start

```csharp
using ResultFlow.Results;
using ResultFlow.AspNetCore;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        return result.ToActionResult();  // ? Simple!
    }
}
```

## ? Features

- ? **Automatic HTTP Status Code Mapping** - Error types automatically map to correct HTTP status codes
- ? **Rich Error Responses** - Includes error code, message, details, and metadata
- ? **Type Safety** - Works with both generic `Result<T>` and non-generic `Result`
- ? **Zero Boilerplate** - Single method call converts results to ActionResults
- ? **Comprehensive Logging** - Errors include detailed metadata for debugging

## ?? How It Works

The `ToActionResult()` extension method automatically:

1. **On Success** ? Returns `200 OK` with the result value
2. **On Failure** ? Returns appropriate HTTP status based on error type:
   - `BadRequestError` ? 400 Bad Request
   - `UnauthorizedError` ? 401 Unauthorized
   - `ForbiddenError` ? 403 Forbidden
   - `NotFoundError` ? 404 Not Found
   - `ConflictError` ? 409 Conflict
   - `ValidationError` ? 422 Unprocessable Entity
   - `InternalServerError` ? 500 Internal Server Error

## ?? Usage Examples

### Retrieve Operation (GET)

```csharp
[HttpGet("{id}")]
[ProducesResponseType(typeof(User), 200)]
[ProducesResponseType(404)]
[ProducesResponseType(500)]
public async Task<IActionResult> GetUser(int id)
{
    var result = await _userService.GetUserByIdAsync(id);
    return result.ToActionResult();
}
```

### Create Operation (POST)

```csharp
[HttpPost]
[ProducesResponseType(typeof(User), 200)]
[ProducesResponseType(400)]
[ProducesResponseType(409)]
[ProducesResponseType(500)]
public async Task<IActionResult> CreateUser(CreateUserRequest request)
{
    var result = await _userService.CreateUserAsync(request);
    return result.ToActionResult();
}
```

### Update Operation (PUT)

```csharp
[HttpPut("{id}")]
[ProducesResponseType(typeof(User), 200)]
[ProducesResponseType(400)]
[ProducesResponseType(404)]
[ProducesResponseType(409)]
[ProducesResponseType(500)]
public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request)
{
    var result = await _userService.UpdateUserAsync(id, request);
    return result.ToActionResult();
}
```

### Delete Operation (DELETE - Void Operation)

```csharp
[HttpDelete("{id}")]
[ProducesResponseType(200)]
[ProducesResponseType(404)]
[ProducesResponseType(500)]
public async Task<IActionResult> DeleteUser(int id)
{
    var result = await _userService.DeleteUserAsync(id);
    return result.ToActionResult();
}
```

## ?? Response Format

### Success Response (200 OK)

```json
{
  "id": 1,
  "name": "John Doe",
  "email": "john@example.com"
}
```

### Error Response (404 Not Found)

```json
{
  "code": "NOT_FOUND",
  "message": "The User with identifier '999' was not found.",
  "metadata": {
    "resourceName": "User",
    "identifier": 999
  }
}
```

### Error Response (400 Bad Request)

```json
{
  "code": "BAD_REQUEST_INVALID_PARAMETER",
  "message": "The parameter 'email' is invalid: Invalid email format",
  "details": "Email must follow pattern: user@example.com",
  "metadata": {
    "parameterName": "email",
    "providedValue": "invalid-email"
  }
}
```

### Error Response (409 Conflict)

```json
{
  "code": "CONFLICT_DUPLICATE_RESOURCE",
  "message": "A Email with the value 'john@example.com' already exists.",
  "metadata": {
    "resourceName": "Email",
    "conflictingValue": "john@example.com"
  }
}
```

### Error Response (500 Internal Server Error)

```json
{
  "code": "INTERNAL_SERVER_ERROR",
  "message": "An internal server error occurred.",
  "details": "System.NullReferenceException: Object reference not set to an instance of an object.",
  "metadata": {
    "exceptionType": "System.NullReferenceException",
    "exceptionMessage": "Object reference not set to an instance of an object."
  }
}
```

## ?? Service Implementation Example

```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly ILogger<UserService> _logger;

    public async Task<Result<User>> GetUserByIdAsync(int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Invalid user ID: {UserId}", id);
            return Result<User>.Failure(
                BadRequestError.ForInvalidParameter("id", "Must be positive", id)
            );
        }

        var user = await _repository.FindByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", id);
            return Result<User>.Failure(
                NotFoundError.ByIdentifier("User", id)
            );
        }

        return Result<User>.Success(user);
    }

    public async Task<Result<User>> CreateUserAsync(CreateUserRequest request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            _logger.LogWarning("Email is required");
            return Result<User>.Failure(
                BadRequestError.ForMissingField("Email")
            );
        }

        if (!IsValidEmail(request.Email))
        {
            _logger.LogWarning("Invalid email format: {Email}", request.Email);
            return Result<User>.Failure(
                BadRequestError.ForInvalidParameter("Email", "Invalid format", request.Email)
            );
        }

        // Check for duplicates
        var existing = await _repository.FindByEmailAsync(request.Email);
        if (existing != null)
        {
            _logger.LogWarning("Email already exists: {Email}", request.Email);
            return Result<User>.Failure(
                ConflictError.ForDuplicateResource("Email", request.Email)
            );
        }

        // Create user
        try
        {
            var user = new User
            {
                Email = request.Email,
                Name = request.Name,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(user);

            _logger.LogInformation("User created: {UserId}", user.Id);
            return Result<User>.Success(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return Result<User>.Failure(
                InternalServerError.FromException(ex, "Error creating user")
            );
        }
    }

    public async Task<Result> DeleteUserAsync(int id)
    {
        var user = await _repository.FindByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", id);
            return Result.Failure(
                NotFoundError.ByIdentifier("User", id)
            );
        }

        try
        {
            await _repository.DeleteAsync(user);
            _logger.LogInformation("User deleted: {UserId}", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", id);
            return Result.Failure(
                InternalServerError.ForOperation("DeleteUser", ex)
            );
        }
    }

    private bool IsValidEmail(string email)
        => email.Contains("@") && email.Contains(".");
}
```

## ?? HTTP Status Code Mapping

| Error Type | HTTP Code | Response | Description |
|-----------|-----------|----------|-------------|
| `BadRequestError` | 400 | Bad Request | Invalid request parameters or format |
| `UnauthorizedError` | 401 | Unauthorized | Missing or invalid authentication |
| `ForbiddenError` | 403 | Forbidden | User lacks required permissions |
| `NotFoundError` | 404 | Not Found | Resource not found |
| `ConflictError` | 409 | Conflict | Resource conflict or duplicate |
| `ValidationError` | 422 | Unprocessable Entity | Validation rule violations |
| `InternalServerError` | 500 | Internal Server Error | Server-side errors |

## ?? API Reference

### ToActionResult - Result<T>

```csharp
/// <summary>
/// Converts a Result<TValue> to an IActionResult with automatic
/// HTTP status code mapping based on the error type.
/// </summary>
/// <typeparam name="TValue">The type of the success value.</typeparam>
/// <param name="result">The result to convert.</param>
/// <returns>An IActionResult representing the result state.</returns>
/// <remarks>
/// On success, returns 200 OK with the value.
/// On failure, returns appropriate HTTP status code based on error type.
/// </remarks>
public static IActionResult ToActionResult<TValue>(this Result<TValue> result)
```

### ToActionResult - Result (Void)

```csharp
/// <summary>
/// Converts a non-generic Result to an IActionResult with automatic
/// HTTP status code mapping based on the error type.
/// </summary>
/// <param name="result">The result to convert.</param>
/// <returns>An IActionResult representing the result state.</returns>
/// <remarks>
/// On success, returns 200 OK.
/// On failure, returns appropriate HTTP status code based on error type.
/// </remarks>
public static IActionResult ToActionResult(this Result result)
```

## ?? Real-World Example: Complete API

```csharp
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService service, ILogger<ProductsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProduct(int id)
    {
        _logger.LogInformation("Getting product with ID: {ProductId}", id);
        var result = await _service.GetProductByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllProducts()
    {
        _logger.LogInformation("Getting all products");
        var result = await _service.GetAllProductsAsync();
        return result.ToActionResult();
    }

    [HttpPost]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateProduct(CreateProductRequest request)
    {
        _logger.LogInformation("Creating new product: {ProductName}", request.Name);
        var result = await _service.CreateProductAsync(request);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductRequest request)
    {
        _logger.LogInformation("Updating product with ID: {ProductId}", id);
        var result = await _service.UpdateProductAsync(id, request);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        _logger.LogInformation("Deleting product with ID: {ProductId}", id);
        var result = await _service.DeleteProductAsync(id);
        return result.ToActionResult();
    }
}
```

## ?? Dependencies

- [Shl.ResultFlow](https://www.nuget.org/packages/Shl.ResultFlow/) - Core Result pattern library
- Microsoft.AspNetCore.Mvc.Core - ASP.NET Core MVC framework

## ?? Contributing

Contributions are welcome! You can:
- ?? Report bugs by creating an [Issue](https://github.com/said1993/ResultFlow/issues)
- ?? Suggest features in [Discussions](https://github.com/said1993/ResultFlow/discussions)
- ?? Submit Pull Requests with improvements

### Development Setup

```bash
git clone https://github.com/said1993/ResultFlow.git
cd ResultFlow
dotnet build
dotnet test
```

## ?? License

MIT - See the [LICENSE](../../LICENSE) file for details.

## ?? Support

Need help?
- ?? Create an [Issue](https://github.com/said1993/ResultFlow/issues)
- ?? Start a [Discussion](https://github.com/said1993/ResultFlow/discussions)
- ?? Visit the [Repository](https://github.com/said1993/ResultFlow)

## ?? Related Packages

- [Shl.ResultFlow](https://www.nuget.org/packages/Shl.ResultFlow/) - Core library
- Shl.ResultFlow.FluentValidation - FluentValidation integration (coming soon)
- Shl.ResultFlow.Logging - Structured logging integration (coming soon)

---

**Made with ?? by [said1993](https://github.com/said1993)**

Updated: 2025-11-23 19:04:08 UTC

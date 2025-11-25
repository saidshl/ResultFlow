# ResultFlow

A simple, lightweight, and powerful implementation of the **Result Pattern** for C# that provides elegant error handling and success result management. Designed for building robust, maintainable applications with clear error flow control.

## 📚 Documentation Navigation

### Core Packages

| Package | Description | Documentation |
|---------|-------------|-----------------|
| **Shl.ResultFlow** | Core Result pattern library with zero dependencies | 📖 [This Document](#ResultFlow) |
| **Shl.ResultFlow.AspNetCore** | ASP.NET Core integration with automatic HTTP status code mapping | 📖 [View Documentation](./source/ResultFlow.AspNetCore/README.md) |
| **Shl.ResultFlow.FluentValidation** | FluentValidation seamless integration for validation results | 📖 [View Documentation](./source/ResultFlow.FluentValidation/README.md) |

**Future Extensions:**
- 🔮 **Shl.ResultFlow.Logging** - Structured logging integration (coming in v1.1)
- 🔮 **Shl.ResultFlow.Testing** - Unit testing assertions (coming in v1.1)

---

## ✨ Features

- ✅ **Type-Safe Result Pattern** - Strongly-typed success and failure handling without exceptions for flow control
- ✅ **Immutable Design** - `readonly struct` implementation for thread-safe, memory-efficient operations
- ✅ **Functional API** - Comprehensive functional programming support with `Map`, `Bind`, `Filter`, `Match`, and `Tap`
- ✅ **Pattern Matching** - Full C# pattern matching support for elegant control flow
- ✅ **Non-Generic Result** - Support for void operations that don't return values
- ✅ **Flexible Error Handling** - Rich error metadata, tracing, and exception wrapping for debugging
- ✅ **Enterprise-Ready** - Optimized for ASP.NET Core with seamless ActionResult conversion
- ✅ **Zero Dependencies** - Core library has no external dependencies
- ✅ **MIT Licensed** - Free for personal and commercial use

## 📦 Installation

Install the NuGet package:

```bash
dotnet add package Shl.ResultFlow
```

Or via Package Manager:

```powershell
Install-Package Shl.ResultFlow
```

## 🚀 Quick Start

### Basic Usage

```csharp
using ResultFlow.Results;
using ResultFlow.Errors;

// Success case
var userResult = Result<User>.Success(new User { Id = 1, Name = "John Doe" });

// Failure case  
var errorResult = Result<User>.Failure(
    NotFoundError.WithDefaults("User not found")
);

// Check status
if (userResult.IsOk)
{
    var user = userResult.Value;
    Console.WriteLine($"User: {user.Name}");
}

if (errorResult.HasError)
{
    Console.WriteLine($"Error: {errorResult.Error?.Message}");
}
```

### Pattern Matching

```csharp
// Match with return value
var message = userResult.Match(
    onSuccess: user => $"Welcome, {user.Name}!",
    onFailure: error => $"Error: {error.Message}"
);
Console.WriteLine(message);

// Match with side effects
userResult.Match(
    onSuccess: user => Console.WriteLine($"Found user: {user.Name}"),
    onFailure: error => Console.WriteLine($"Error: {error.Message}")
);
```

### Void Operations

```csharp
// For operations that don't return a value
public Result DeleteUser(int id)
{
    var user = _database.FindUser(id);
    if (user == null)
        return Result.Failure(NotFoundError.WithDefaults("User not found"));
    
    _database.Delete(user);
    return Result.Success();
}

// Usage
var deleteResult = DeleteUser(1);
deleteResult.Match(
    onSuccess: () => Console.WriteLine("User deleted successfully"),
    onFailure: error => Console.WriteLine($"Error: {error.Message}")
);
```

## 📖 Core Concepts

### Error Handling

ResultFlow provides multiple ways to create and handle errors:

#### **1. PredefinedErrors - Quick Factory Methods**

```csharp
// Simple, quick error creation
var notFoundError = PredefinedErrors.NotFound("User");
var badRequestError = PredefinedErrors.BadRequest("Invalid input");
var conflictError = PredefinedErrors.Conflict("Email already exists");
var authError = PredefinedErrors.Unauthorized();
var validationError = PredefinedErrors.ValidationFailed("Multiple validation errors");
var serverError = PredefinedErrors.InternalError("Database connection failed");

return Result<User>.Failure(notFoundError);
```

#### **2. Error Type Factory Methods - Specific Scenarios**

```csharp
// NotFoundError with context
var error = NotFoundError.ForResource("Product", resourceId: "PROD-123");
// Message: "The requested Product was not found."
// Metadata: { resourceName: "Product", resourceId: "PROD-123" }

// BadRequestError with field context
var error = BadRequestError.ForInvalidParameter(
    parameterName: "email",
    reason: "Invalid email format",
    providedValue: "not-an-email"
);
// Message: "The parameter 'email' is invalid: Invalid email format"
// Metadata: { parameterName: "email", providedValue: "not-an-email" }

// ConflictError with version info
var error = ConflictError.ForVersionMismatch(
    resourceName: "Document",
    expectedVersion: "v1",
    currentVersion: "v2"
);
// Message: "The Document has been modified. Expected version: v1, Current version: v2."

// ForbiddenError with role info
var error = ForbiddenError.ForMissingRole(
    requiredRole: "Admin",
    userRole: "User"
);
// Message: "This operation requires the 'Admin' role."
// Metadata: { requiredRole: "Admin", userRole: "User" }

// InternalServerError with exception
var error = InternalServerError.FromException(
    dbException,
    message: "Database query failed"
);
// Automatically includes exception type, message, and stack trace in metadata
```

#### **3. ErrorBuilder - Complex Custom Errors**

```csharp
var error = ErrorBuilder
    .Create("PAYMENT_FAILED", "Payment processing failed")
    .WithDetails("Credit card was declined by the payment gateway")
    .AddMetadata("orderId", order.Id)
    .AddMetadata("amount", order.Total)
    .AddMetadata("paymentGateway", "Stripe")
    .AddMetadata("timestamp", DateTime.UtcNow)
    .AddMetadata("retryable", true)
    .WithException(stripeException)
    .Build();

return Result<PaymentResult>.Failure(error);
```

### Functional Operations

#### **Map - Transform Values**

```csharp
var userIdResult = Result<int>.Success(42);

var userResult = userIdResult
    .Map(id => _userService.GetUserById(id))
    .Map(user => user.Email)
    .Map(email => new EmailDto { Address = email });

// Chain transformations with error propagation
// If any step fails, error is returned immediately
```

#### **Bind - Chain Operations**

```csharp
public Result<User> GetUser(int id)
{
    if (id <= 0)
        return Result<User>.Failure(
            BadRequestError.ForInvalidParameter("id", "Must be positive")
        );
    
    var user = _database.FindUser(id);
    return user != null
        ? Result<User>.Success(user)
        : Result<User>.Failure(NotFoundError.ForResource("User", id.ToString()));
}

public Result<string> GetUserEmail(int id)
{
    return GetUser(id)
        .Bind(user => ValidateUserAsync(user))
        .Bind(user => AuthorizeUserAsync(user))
        .Map(user => user.Email);
}

// Result: Success("john@example.com") or any error from the chain
```

#### **Filter - Conditional Logic**

```csharp
var ageResult = Result<int>.Success(25);

var validAge = ageResult.Filter(
    age => age >= 18,
    new ValidationError("UNDERAGE", "User must be 18 or older")
);

if (validAge.IsOk)
{
    Console.WriteLine("User is old enough");
}
```

#### **Tap - Side Effects**

```csharp
var userResult = Result<User>.Success(new User { Id = 1, Name = "John" });

var result = userResult
    .Tap(user => Console.WriteLine($"Processing user: {user.Name}"))
    .Tap(user => _logger.LogInformation($"User ID: {user.Id}"))
    .Tap(user => _cache.Set($"user:{user.Id}", user))
    .TapError(error => _logger.LogError($"Error: {error.Message}"))
    .Map(user => user.Email);

// Tap executes side effects without changing the result
```

#### **Then - Sequential Operations**

```csharp
var result = ValidateUser(user)
    .Then(user => SaveUserToDatabase(user))
    .Then(user => SendWelcomeEmail(user))
    .Then(user => LogUserCreation(user));

// Then chains operations sequentially
// Short-circuits on first failure
```

### Async Operations

ResultFlow provides seamless support for async operations through `Task<Result<T>>` and `Task<VoidResult>` extensions. This allows you to chain async methods without manually awaiting them at each step.

#### **MapAsync & BindAsync - Async Chaining**

```csharp
public async Task<Result<UserDto>> GetUserDtoAsync(int userId)
{
    return await _repository.GetUserAsync(userId)
        .BindAsync(user => _validator.ValidateAsync(user))
        .MapAsync(user => _mapper.MapToDtoAsync(user));
}
```

#### **TapAsync - Async Side Effects**

```csharp
public async Task<Result<User>> CreateUserAsync(User user)
{
    return await ValidateUserAsync(user)
        .TapAsync(u => _logger.LogInformationAsync($"Creating user {u.Name}"))
        .TapAsync(u => _repository.AddAsync(u))
        .TapErrorAsync(e => _logger.LogErrorAsync($"Failed: {e.Message}"));
}
```

#### **MatchAsync - Async Pattern Matching**

```csharp
var result = await GetUserAsync(1);

await result.MatchAsync(
    onSuccess: async user => await SendEmailAsync(user),
    onFailure: async error => await LogErrorAsync(error)
);
```

### Error Value Retrieval

```csharp
var userResult = GetUser(123);

// Option 1: Get value or default
var user = userResult.GetValueOrDefault(new User { Id = 0, Name = "Unknown" });

// Option 2: Get value or execute fallback
var user = userResult.GetValueOrElse(error => 
{
    _logger.LogError($"Failed to get user: {error.Message}");
    return new User { Id = 0, Name = "Unknown" };
});

// Option 3: Get value or throw exception
var user = userResult.GetValueOrThrow();
// Throws InvalidOperationException if failed

// Option 4: Get value with custom exception
var user = userResult.GetValueOrThrow(error => 
    new UserNotFoundException($"User not found: {error.Message}", error.InnerException)
);
```

## 🎯 Error Types

ResultFlow includes comprehensive HTTP error types with factory methods:

| Error Type | HTTP Code | Factory Methods | Use Case |
|-----------|-----------|-----------------|----------|
| **BadRequestError** | 400 | `WithDefaults()`, `ForInvalidParameter()`, `ForMissingField()`, `ForInvalidFormat()` | Invalid request parameters or format |
| **UnauthorizedError** | 401 | `WithDefaults()`, `ForReason()` | Missing or invalid authentication |
| **ForbiddenError** | 403 | `WithDefaults()`, `ForMissingRole()`, `ForMissingPermission()` | User lacks required permissions |
| **NotFoundError** | 404 | `WithDefaults()`, `ForResource()`, `ByIdentifier()` | Resource not found |
| **ConflictError** | 409 | `WithDefaults()`, `ForDuplicateResource()`, `ForVersionMismatch()`, `ForStateConflict()` | Resource conflict or duplicate |
| **ValidationError** | 422 | `WithDefaults()`, `ForField()` | Validation rule violations |
| **InternalServerError** | 500 | `WithDefaults()`, `FromException()`, `ForOperation()`, `WithCode()` | Server-side errors |

### Creating Custom Errors

```csharp
// Using ErrorCodes constants
var error = new Error(
    code: ErrorCodes.BadRequest.InvalidParameter,
    message: "Email parameter is invalid",
    details: "Email must follow pattern: user@example.com",
    metadata: new Dictionary<string, object>
    {
        { "fieldName", "email" },
        { "expectedFormat", "user@example.com" },
        { "providedValue", "invalid-email" }
    }
);

// Using ErrorBuilder for complex scenarios
var error = ErrorBuilder
    .Create(ErrorCodes.Custom.BusinessLogicError, "Order cannot be processed")
    .WithDetails("Customer credit limit exceeded")
    .AddMetadata("customerId", 123)
    .AddMetadata("creditLimit", 5000)
    .AddMetadata("orderTotal", 6000)
    .AddMetadata("exceededBy", 1000)
    .Build();
```

## 💡 Real-World Examples

### Example 1: User Service with Validation

```csharp
public class UserService
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
            return Result<User>.Failure(
                BadRequestError.ForMissingField("Email")
            );

        if (!IsValidEmail(request.Email))
            return Result<User>.Failure(
                BadRequestError.ForInvalidParameter("Email", "Invalid format", request.Email)
            );

        // Check for duplicates
        var existing = await _repository.FindByEmailAsync(request.Email);
        if (existing != null)
            return Result<User>.Failure(
                ConflictError.ForDuplicateResource("Email", request.Email)
            );

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
            return Result.Failure(
                NotFoundError.ByIdentifier("User", id)
            );

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

### Example 2: Complex Business Logic with Chaining

```csharp
public async Task<Result<OrderDto>> ProcessOrderAsync(int orderId)
{
    return await GetOrderAsync(orderId)
        .Bind(order => ValidateOrderAsync(order))
        .Bind(order => CheckInventoryAsync(order))
        .Bind(order => ProcessPaymentAsync(order))
        .Bind(order => CreateShipmentAsync(order))
        .Bind(order => SendConfirmationEmailAsync(order))
        .Tap(order => _logger.LogInformation("Order processed: {OrderId}", order.Id))
        .TapError(error => _logger.LogError("Order processing failed: {Error}", error.Message))
        .Map(order => _mapper.Map<OrderDto>(order));
}

private async Task<Result<Order>> GetOrderAsync(int id)
{
    var order = await _repository.GetOrderAsync(id);
    return order != null
        ? Result<Order>.Success(order)
        : Result<Order>.Failure(NotFoundError.ForResource("Order", id.ToString()));
}

private async Task<Result<Order>> ValidateOrderAsync(Order order)
{
    if (order.Items.Count == 0)
        return Result<Order>.Failure(
            ValidationError.WithDefaults("Order must contain at least one item")
        );

    return Result<Order>.Success(order);
}

private async Task<Result<Order>> CheckInventoryAsync(Order order)
{
    foreach (var item in order.Items)
    {
        var stock = await _inventoryService.GetStockAsync(item.ProductId);
        if (stock < item.Quantity)
            return Result<Order>.Failure(
                ConflictError.WithDefaults($"Insufficient stock for product {item.ProductId}")
            );
    }

    return Result<Order>.Success(order);
}

private async Task<Result<Order>> ProcessPaymentAsync(Order order)
{
    try
    {
        await _paymentService.ChargeAsync(order.CustomerId, order.Total);
        return Result<Order>.Success(order);
    }
    catch (PaymentException ex)
    {
        return Result<Order>.Failure(
            InternalServerError.ForOperation("ProcessPayment", ex)
        );
    }
}

// ... more methods
```

### Example 3: Validation with Filter

```csharp
public Result<User> CreateUser(UserRequest request)
{
    return ValidateUserRequest(request)
        .Filter(
            user => user.Age >= 18,
            ValidationError.WithDefaults("User must be 18 years old")
        )
        .Filter(
            user => !user.Email.Contains("invalid"),
            BadRequestError.ForInvalidParameter("Email", "Invalid email domain")
        )
        .Tap(user => _repository.Add(user))
        .Tap(user => _logger.LogInformation("User created: {UserId}", user.Id));
}

private Result<User> ValidateUserRequest(UserRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Name))
        return Result<User>.Failure(
            BadRequestError.ForMissingField("Name")
        );

    if (string.IsNullOrWhiteSpace(request.Email))
        return Result<User>.Failure(
            BadRequestError.ForMissingField("Email")
        );

    return Result<User>.Success(new User 
    { 
        Name = request.Name, 
        Email = request.Email,
        Age = request.Age
    });
}
```

## 📚 API Reference

### Result<TValue>

```csharp
// Factory Methods
Result<T> Success(T value)              // Create success result
Result<T> Failure(Error error)          // Create failure result

// Properties
T? Value                                // Success value
Error? Error                            // Error information
bool IsOk                              // Check if successful
bool HasError                          // Check if failed

// Functional Methods
Result<TResult> Map<TResult>(Func<T, TResult> map)
Result<TResult> Bind<TResult>(Func<T, Result<TResult>> bind)
Result<TResult> Filter(Func<T, bool> predicate, Error error)
Result<T> Tap(Action<T> action)
Result<T> TapError(Action<Error> action)
Result<TResult> Then<TResult>(Result<TResult> other)
Result<TResult> Then<TResult>(Func<T, Result<TResult>> then)

// Pattern Matching
TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)
void Match(Action<T> onSuccess, Action<Error> onFailure)

// Value Retrieval
T? GetValueOrDefault(T? defaultValue = default)
T? GetValueOrElse(Func<Error, T?> onFailure)
T? GetValueOrThrow()
T? GetValueOrThrow(Func<Error, Exception> exceptionFactory)

// Equality
bool Equals(Result<T> other)
override int GetHashCode()
override string ToString()
```

### Result (Void)

```csharp
// Factory Methods
Result Success()                        // Create success result
Result Failure(Error error)             // Create failure result

// Properties
Error? Error                            // Error information
bool IsOk                              // Check if successful
bool HasError                          // Check if failed

// Functional Methods
Result Tap(Action action)
Result TapError(Action<Error> action)

// Pattern Matching
void Match(Action onSuccess, Action<Error> onFailure)
TResult Match<TResult>(Func<TResult> onSuccess, Func<Error, TResult> onFailure)

// Utility
Error? GetErrorOrThrow()
override string ToString()
```

### Error

```csharp
// Properties
string Code                             // Error code identifier
string Message                          // Human-readable message
string? Details                         // Additional details
Dictionary<string, object>? Metadata    // Custom metadata
Exception? InnerException               // Underlying exception
```

### ErrorBuilder

```csharp
ErrorBuilder WithCode(string code)
ErrorBuilder WithMessage(string message)
ErrorBuilder WithDetails(string details)
ErrorBuilder WithMetadata(Dictionary<string, object> metadata)
ErrorBuilder AddMetadata(string key, object value)
ErrorBuilder AddMetadataRange(Dictionary<string, object> metadata)
ErrorBuilder WithException(Exception exception)
ErrorBuilder ClearMetadata()
Error Build()
TError Build<TError>() where TError : Error, new()

static ErrorBuilder Create(string code, string message)
static ErrorBuilder Empty()
```

### Task Extensions

Extensions for `Task<Result<T>>` and `Task<VoidResult>`:

```csharp
// Async Transformations
Task<Result<TNew>> MapAsync<T, TNew>(this Task<Result<T>>, Func<T, Task<TNew>>)
Task<Result<TNew>> BindAsync<T, TNew>(this Task<Result<T>>, Func<T, Task<Result<TNew>>>)

// Async Side Effects
Task<Result<T>> TapAsync<T>(this Task<Result<T>>, Func<T, Task>)
Task<Result<T>> TapErrorAsync<T>(this Task<Result<T>>, Func<Error, Task>)

// Async Pattern Matching
Task<TResult> MatchAsync<T, TResult>(this Task<Result<T>>, Func<T, Task<TResult>>, Func<Error, Task<TResult>>)
```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit issues or pull requests on [GitHub](https://github.com/said1993/ResultFlow).

### Development Setup

```bash
git clone https://github.com/said1993/ResultFlow.git
cd ResultFlow
dotnet build
dotnet test
```

## 💬 Support

For questions, issues, or suggestions:
- 🐛 Create an [Issue](https://github.com/said1993/ResultFlow/issues)
- 💡 Start a [Discussion](https://github.com/said1993/ResultFlow/discussions)
- 📦 Visit [Repository](https://github.com/said1993/ResultFlow)

---

**Made with ❤️ by [said1993](https://github.com/said1993)**

Last updated: 2025-11-23 10:13:05 UTC

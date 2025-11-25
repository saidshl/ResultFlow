# ResultFlow Samples

This directory contains sample projects demonstrating various usage patterns of ResultFlow.

## ?? Sample Projects

### 1. [BasicUsage](./ResultFlow.Samples.BasicUsage/) - Console Application
**Target Framework:** .NET 8.0  
**Dependencies:** ResultFlow

Demonstrates fundamental ResultFlow concepts:
- ? Creating success/failure results
- ? Built-in error types (NotFoundError, ValidationError, etc.)
- ? Pattern matching with `Match()`
- ? Railway-oriented programming with `Then()`, `Bind()`, `Map()`
- ? Async operations with `Task<Result<T>>`
- ? Error builder pattern
- ? Chaining and transformations

**Run:**
```bash
dotnet run --project samples/ResultFlow.Samples.BasicUsage
```

---

### 2. [WebApi](./ResultFlow.Samples.WebApi/) - ASP.NET Core Web API
**Target Framework:** .NET 8.0  
**Dependencies:** ResultFlow, ResultFlow.AspNetCore

Full-featured REST API demonstrating:
- ? Automatic error-to-HTTP status mapping
- ? Clean controller design with `ToActionResult()`
- ? Repository and service layer patterns
- ? Railway-oriented programming in services
- ? Structured error responses
- ? Swagger/OpenAPI integration
- ? CRUD operations with consistent error handling

**Run:**
```bash
dotnet run --project samples/ResultFlow.Samples.WebApi
# Then navigate to: https://localhost:5001/swagger
```

**Example API Calls:**
```bash
# Get all users
curl -X GET https://localhost:5001/api/users

# Get user by ID
curl -X GET https://localhost:5001/api/users/1

# Create user
curl -X POST https://localhost:5001/api/users \
  -H "Content-Type: application/json" \
  -d '{"name":"Alice","email":"alice@example.com"}'

# Update user
curl -X PUT https://localhost:5001/api/users/1 \
  -H "Content-Type: application/json" \
  -d '{"name":"Alice Updated","email":"alice@example.com"}'

# Delete user
curl -X DELETE https://localhost:5001/api/users/1
```

---

### 3. [FluentValidation](./ResultFlow.Samples.FluentValidation/) - Console Application
**Target Framework:** .NET 8.0  
**Dependencies:** ResultFlow, ResultFlow.FluentValidation, FluentValidation

Integration with FluentValidation:
- ? Seamless FluentValidation integration
- ? Automatic conversion from `ValidationResult` to `Result<T>`
- ? Field-specific error details
- ? Complex validation rules
- ? Service layer validation patterns
- ? Combining validation with business logic

**Run:**
```bash
dotnet run --project samples/ResultFlow.Samples.FluentValidation
```

---

## ?? Error-to-HTTP Status Mapping (WebApi Sample)

| Error Type | HTTP Status | Example Scenario |
|------------|-------------|------------------|
| `NotFoundError` | 404 Not Found | User ID doesn't exist |
| `ValidationError` | 422 Unprocessable Entity | Invalid email format |
| `BadRequestError` | 400 Bad Request | Malformed request |
| `UnauthorizedError` | 401 Unauthorized | Missing/invalid authentication |
| `ForbiddenError` | 403 Forbidden | Insufficient permissions |
| `ConflictError` | 409 Conflict | Duplicate email address |
| `InternalServerError` | 500 Internal Server Error | Unexpected server error |
| `TooManyRequestsError` | 429 Too Many Requests | Rate limit exceeded |

---

## ?? Quick Start

### Clone and Run All Samples

```bash
# Clone the repository
git clone https://github.com/saidshl/ResultFlow.git
cd ResultFlow

# Run BasicUsage sample
dotnet run --project samples/ResultFlow.Samples.BasicUsage

# Run WebApi sample (with Swagger UI)
dotnet run --project samples/ResultFlow.Samples.WebApi

# Run FluentValidation sample
dotnet run --project samples/ResultFlow.Samples.FluentValidation
```

---

## ?? Common Patterns Demonstrated

### 1. Railway-Oriented Programming
```csharp
var result = await GetUser(id)
    .Tap(user => Log($"Retrieved {user.Name}"))
    .Then(user => ValidateUser(user))
    .Then(user => SaveUser(user))
    .TapError(error => LogError(error));
```

### 2. Controller Pattern (ASP.NET Core)
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
{
    return await _userService.GetUserAsync(id)
        .ToActionResultAsync(); // Automatic error mapping!
}
```

### 3. FluentValidation Integration
```csharp
public async Task<Result<User>> CreateUserAsync(CreateUserRequest request)
{
    var validationResult = await _validator.ValidateAsync(request);
    if (!validationResult.IsValid)
    {
        return validationResult.ToResult<User>();
    }
    
    // Continue with business logic...
}
```

### 4. Pattern Matching
```csharp
result.Match(
    onSuccess: user => Console.WriteLine($"Success: {user.Name}"),
    onFailure: error => Console.WriteLine($"Error: {error.Message}")
);
```

### 5. Transformation (Map & Bind)
```csharp
var result = await GetUser(id)
    .MapAsync(user => user.Name.ToUpper())
    .FilterAsync(name => name.Length > 3, new Error("TOO_SHORT", "Name too short"))
    .BindAsync(name => SaveProcessedName(name));
```

---

## ?? Building All Samples

```bash
# Build all samples
dotnet build samples/

# Or build individually
dotnet build samples/ResultFlow.Samples.BasicUsage
dotnet build samples/ResultFlow.Samples.WebApi
dotnet build samples/ResultFlow.Samples.FluentValidation
```

---

## ?? Learning Path

1. **Start with BasicUsage** - Learn core concepts and patterns
2. **Explore WebApi** - See real-world API implementation
3. **Study FluentValidation** - Understand validation integration

Each sample includes:
- ? Comprehensive code examples
- ? Detailed comments
- ? README with explanations
- ? Common patterns and best practices

---

## ?? Contributing

Have a sample idea? Found an issue? Feel free to open an issue or submit a PR!

---

## ?? License

These samples are part of the ResultFlow project and are licensed under the MIT License.

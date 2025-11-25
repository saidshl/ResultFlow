# ResultFlow.Samples.WebApi

ASP.NET Core Web API demonstrating ResultFlow integration with automatic error-to-HTTP status code mapping.

## Features Demonstrated

- ? **Automatic HTTP Status Mapping** - Error types automatically map to appropriate HTTP status codes
- ? **Railway-Oriented Programming** - Chain operations with `.Then()`, `.Bind()`, `.Tap()`
- ? **Clean Controller Code** - Controllers are thin, business logic in services
- ? **Consistent Error Responses** - All errors return structured JSON
- ? **Async/Await Support** - Full async support with `Task<Result<T>>`

## Running the Sample

```bash
dotnet run --project samples/ResultFlow.Samples.WebApi
```

Then navigate to `https://localhost:5001/swagger` to see the Swagger UI.

## Error-to-HTTP Status Mapping

ResultFlow.AspNetCore automatically maps error types to HTTP status codes:

| Error Type | HTTP Status | Code |
|------------|-------------|------|
| `NotFoundError` | 404 Not Found | `NOT_FOUND` |
| `BadRequestError` | 400 Bad Request | `BAD_REQUEST` |
| `ValidationError` | 422 Unprocessable Entity | `VALIDATION_ERROR` |
| `UnauthorizedError` | 401 Unauthorized | `UNAUTHORIZED` |
| `ForbiddenError` | 403 Forbidden | `FORBIDDEN` |
| `ConflictError` | 409 Conflict | `CONFLICT` |
| `InternalServerError` | 500 Internal Server Error | `INTERNAL_SERVER_ERROR` |

## Example Requests

### Get All Users
```bash
curl -X GET https://localhost:5001/api/users
```

### Get User by ID
```bash
curl -X GET https://localhost:5001/api/users/1
```

### Create User
```bash
curl -X POST https://localhost:5001/api/users \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Alice Johnson",
    "email": "alice@example.com"
  }'
```

### Update User
```bash
curl -X PUT https://localhost:5001/api/users/1 \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Updated",
    "email": "john.updated@example.com"
  }'
```

### Delete User
```bash
curl -X DELETE https://localhost:5001/api/users/1
```

## Error Response Format

All errors return a consistent JSON structure:

```json
{
  "code": "NOT_FOUND",
  "message": "The User with identifier '999' was not found.",
  "details": null,
  "metadata": {
    "resourceName": "User",
    "identifier": 999
  },
  "timestamp": "2024-01-15T10:30:00Z"
}
```

## Key Code Patterns

### Controller Pattern
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
{
    // ResultFlow automatically converts to IActionResult
    return await _userService.GetUserAsync(id)
        .ToActionResultAsync();
}
```

### Service Pattern with Railway-Oriented Programming
```csharp
public async Task<Result<User>> UpdateUserAsync(int id, UpdateUserRequest request)
{
    return await _repository.GetByIdAsync(id)
        .BindAsync(async existingUser =>
        {
            // Validate, update, save
            return await _repository.UpdateAsync(existingUser);
        });
}
```

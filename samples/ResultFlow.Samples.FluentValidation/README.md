# ResultFlow.Samples.FluentValidation

Demonstrates integration between FluentValidation and ResultFlow for seamless validation error handling.

## Features Demonstrated

- ? **FluentValidation Integration** - Use FluentValidation validators with Result pattern
- ? **Automatic Conversion** - ValidationResult automatically converts to Result<T>
- ? **Structured Error Details** - Validation errors include field-specific details
- ? **Service Layer Validation** - Clean validation in business logic
- ? **Railway-Oriented Programming** - Chain validation with other operations

## Running the Sample

```bash
dotnet run --project samples/ResultFlow.Samples.FluentValidation
```

## Key Integration Points

### 1. Validator Definition (Standard FluentValidation)

```csharp
public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(18).WithMessage("Must be at least 18 years old");
    }
}
```

### 2. Validation in Service Layer

```csharp
public async Task<Result<User>> CreateUserAsync(CreateUserRequest request)
{
    // ValidateAsync returns Result<T> automatically
    var validationResult = await _validator.ValidateAsync(request);
    if (!validationResult.IsValid)
    {
        // Convert FluentValidation result to ResultFlow Result
        return validationResult.ToResult<User>();
    }

    // Continue with business logic...
    return Result<User>.Ok(user);
}
```

### 3. Railway-Oriented Validation

```csharp
return await Task.FromResult(GetUser(id))
    .BindAsync(async user =>
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return validationResult.ToResult<User>();
        }

        // Update user 
        return Result<User>.Ok(updatedUser);
    });
```

## Error Response Structure

When validation fails, the error includes all field-specific errors:

```json
{
  "code": "UNPROCESSABLE_ENTITY_VALIDATION",
  "message": "Validation failed",
  "details": "{\"Name\":[\"Name is required\",\"Name must be at least 2 characters\"],\"Email\":[\"Invalid email format\"],\"Age\":[\"Must be at least 18 years old\"]}",
  "metadata": {
    "errors": {
      "Name": ["Name is required", "Name must be at least 2 characters"],
      "Email": ["Invalid email format"],
      "Age": ["Must be at least 18 years old"]
    }
  }
}
```

## Benefits

1. **Type Safety** - Strongly-typed validation with FluentValidation
2. **Consistent Errors** - All errors (validation + business logic) use the same Result pattern
3. **Clean Code** - No try-catch blocks, clear success/failure paths
4. **Composable** - Easy to chain validation with other operations
5. **Testable** - Easy to unit test validation logic

## Example Output

```
=== ResultFlow FluentValidation Integration Examples ===

--- Example 1: Basic Validation ---
? Valid user: John Doe
? Validation failed: Validation failed
  Validation errors:
    - Name: Name is required, Name must be at least 2 characters
    - Email: Invalid email format
    - Age: Must be at least 18 years old

--- Example 2: Complex Validation ---
? Validation failed: Validation failed
Details: {"Name":["Name must be at least 2 characters"],"Age":["Invalid age"],"Website":["Invalid URL format"]}

--- Example 3: Service Layer Integration ---
? User created: ID=1, Name=Alice Johnson
? Expected conflict: A User with the value 'alice@example.com' already exists.
? Validation failed: Validation failed
  Details: {"Name":["Name must be at least 2 characters"],"Email":["Invalid email format"],"Age":["Must be at least 18 years old"]}

# ResultFlow.Samples.BasicUsage

This console application demonstrates the basic usage patterns of ResultFlow.

## What's Covered

1. **Basic Success/Failure** - Creating and working with `Result<T>`
2. **Built-in Error Types** - Using NotFoundError, ValidationError, BadRequestError, etc.
3. **Pattern Matching** - Using `Match()` for success/failure handling
4. **Chaining Operations** - Railway-oriented programming with `Then()`, `Tap()`
5. **Map and Bind** - Transforming result values
6. **Async Operations** - Working with `Task<Result<T>>`
7. **Error Builder** - Building complex errors fluently

## Running the Sample

```bash
dotnet run --project samples/ResultFlow.Samples.BasicUsage
```

## Key Concepts

### Creating Results
```csharp
// Success
var success = Result<int>.Ok(42);

// Failure
var failure = Result<int>.Failed(new Error("CODE", "Message"));

// Implicit conversion
Result<string> result = "Hello";
Result<string> error = new Error("ERR", "Failed");
```

### Pattern Matching
```csharp
result.Match(
    onSuccess: value => Console.WriteLine($"Success: {value}"),
    onFailure: error => Console.WriteLine($"Error: {error.Message}")
);
```

### Railway-Oriented Programming
```csharp
var result = GetUser(1)
    .Tap(user => Log(user))
    .Then(user => ValidateUser(user))
    .Then(user => SaveUser(user));
```

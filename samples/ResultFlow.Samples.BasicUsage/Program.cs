using ResultFlow.Errors;
using ResultFlow.Errors.Builders;
using ResultFlow.Extensions;
using ResultFlow.Results;

namespace ResultFlow.Samples.BasicUsage;

/// <summary>
/// Demonstrates basic usage of the Result pattern with ResultFlow
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== ResultFlow Basic Usage Examples ===\n");

        // Example 1: Basic Success/Failure
        BasicSuccessFailureExample();

        // Example 2: Error Types
        ErrorTypesExample();

        // Example 3: Pattern Matching
        PatternMatchingExample();

        // Example 4: Chaining Operations
        ChainingOperationsExample();

        // Example 5: Map and Bind
        MapAndBindExample();

        // Example 6: Async Operations
        await AsyncOperationsExample();

        // Example 7: Error Builder
        ErrorBuilderExample();

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    #region Example 1: Basic Success/Failure
    static void BasicSuccessFailureExample()
    {
        Console.WriteLine("--- Example 1: Basic Success/Failure ---");

        // Success result
        var successResult = Result<int>.Ok(42);
        Console.WriteLine($"Success: IsOk={successResult.IsOk}, Value={successResult.Value}");

        // Failure result
        var failureResult = Result<int>.Failed(new Error("ERROR_CODE", "Something went wrong"));
        Console.WriteLine($"Failure: HasError={failureResult.HasError}, Error={failureResult.Error?.Message}");

        // Implicit conversions
        Result<string> implicitSuccess = "Hello World";
        Result<string> implicitFailure = new Error("ERR_001", "Error message");

        Console.WriteLine();
    }
    #endregion

    #region Example 2: Error Types
    static void ErrorTypesExample()
    {
        Console.WriteLine("--- Example 2: Built-in Error Types ---");

        // Not Found Error
        var userNotFound = NotFoundError.ForResource("User", "123");
        var result1 = Result<User>.Failed(userNotFound);
        Console.WriteLine($"NotFound: {result1.Error?.Message}");

        // Validation Error
        var validationError = ValidationError.ForField("Email", "Invalid email format");
        var result2 = Result<User>.Failed(validationError);
        Console.WriteLine($"Validation: {result2.Error?.Message}");

        // Bad Request Error
        var badRequest = BadRequestError.ForInvalidParameter("age", "Must be positive", -5);
        var result3 = Result<User>.Failed(badRequest);
        Console.WriteLine($"BadRequest: {result3.Error?.Message}");

        // Unauthorized Error
        var unauthorized = UnauthorizedError.WithDefaults();
        var result4 = Result<User>.Failed(unauthorized);
        Console.WriteLine($"Unauthorized: {result4.Error?.Message}");

        // Conflict Error
        var conflict = ConflictError.ForDuplicateResource("User", "john@example.com");
        var result5 = Result<User>.Failed(conflict);
        Console.WriteLine($"Conflict: {result5.Error?.Message}");

        Console.WriteLine();
    }
    #endregion

    #region Example 3: Pattern Matching
    static void PatternMatchingExample()
    {
        Console.WriteLine("--- Example 3: Pattern Matching ---");

        var result = DivideNumbers(10, 2);

        // Match with return value
        var message = result.Match(
            onSuccess: value => $"Result: {value}",
            onFailure: error => $"Error: {error.Message}"
        );
        Console.WriteLine(message);

        // Match with actions
        result.Match(
            onSuccess: value => Console.WriteLine($"✓ Division successful: {value}"),
            onFailure: error => Console.WriteLine($"✗ Division failed: {error.Message}")
        );

        // Try division by zero
        var errorResult = DivideNumbers(10, 0);
        errorResult.Match(
            onSuccess: value => Console.WriteLine($"Result: {value}"),
            onFailure: error => Console.WriteLine($"Error: {error.Message}")
        );

        Console.WriteLine();
    }

    static Result<double> DivideNumbers(double a, double b)
    {
        if (b == 0)
            return Result<double>.Failed(new Error("DIVISION_BY_ZERO", "Cannot divide by zero"));

        return Result<double>.Ok(a / b);
    }
    #endregion

    #region Example 4: Chaining Operations
    static void ChainingOperationsExample()
    {
        Console.WriteLine("--- Example 4: Chaining Operations (Railway-Oriented) ---");

        var result = GetUser(1)
            .Tap(user => Console.WriteLine($"  ✓ User retrieved: {user.Name}"))
            .Then(user => ValidateUser(user))
            .Tap(user => Console.WriteLine($"  ✓ User validated: {user.Name}"))
            .Then(user => SaveUser(user))
            .Tap(user => Console.WriteLine($"  ✓ User saved: {user.Name}"));

        result.Match(
            onSuccess: user => Console.WriteLine($"Final Success: User {user.Name} processed"),
            onFailure: error => Console.WriteLine($"Final Error: {error.Message}")
        );

        Console.WriteLine();
    }

    static Result<User> GetUser(int id)
    {
        return Result<User>.Ok(new User { Id = id, Name = "John Doe", Email = "john@example.com" });
    }

    static Result<User> ValidateUser(User user)
    {
        if (string.IsNullOrEmpty(user.Email))
            return ValidationError.ForField("Email", "Email is required");

        return Result<User>.Ok(user);
    }

    static Result<User> SaveUser(User user)
    {
        // Simulate save
        return Result<User>.Ok(user);
    }
    #endregion

    #region Example 5: Map and Bind
    static void MapAndBindExample()
    {
        Console.WriteLine("--- Example 5: Map and Bind Transformations ---");

        // Map: Transform success value
        var numberResult = Result<int>.Ok(5);
        var doubledResult = numberResult.Map(x => x * 2);
        Console.WriteLine($"Map: {numberResult.Value} → {doubledResult.Value}");

        // Bind: Chain operations that return Result
        var userResult = Result<int>.Ok(1)
            .Bind(id => GetUser(id))
            .Map(user => user.Name);

        Console.WriteLine($"Bind chain result: {userResult.Value}");

        // Filter: Conditional validation
        var ageResult = Result<int>.Ok(25)
            .Filter(
                age => age >= 18,
                new Error("AGE_INVALID", "Must be 18 or older")
            );

        Console.WriteLine($"Filter (valid): {ageResult.IsOk}");

        var youngAgeResult = Result<int>.Ok(15)
            .Filter(
                age => age >= 18,
                new Error("AGE_INVALID", "Must be 18 or older")
            );

        Console.WriteLine($"Filter (invalid): HasError={youngAgeResult.HasError}");

        Console.WriteLine();
    }
    #endregion

    #region Example 6: Async Operations
    static async Task AsyncOperationsExample()
    {
        Console.WriteLine("--- Example 6: Async Operations ---");

        var result = await GetUserAsync(1)
            .MapAsync(async user =>
            {
                await Task.Delay(100); // Simulate async work
                return user.Name.ToUpper();
            })
            .TapAsync(async name =>
            {
                await Task.Delay(50);
                Console.WriteLine($"  ✓ Async processing: {name}");
            });

        result.Match(
            onSuccess: name => Console.WriteLine($"Final result: {name}"),
            onFailure: error => Console.WriteLine($"Error: {error.Message}")
        );

        Console.WriteLine();
    }

    static async Task<Result<User>> GetUserAsync(int id)
    {
        await Task.Delay(100); // Simulate database call
        return Result<User>.Ok(new User { Id = id, Name = "Jane Doe", Email = "jane@example.com" });
    }
    #endregion

    #region Example 7: Error Builder
    static void ErrorBuilderExample()
    {
        Console.WriteLine("--- Example 7: Error Builder ---");

        var error = new ErrorBuilder()
            .WithCode("CUSTOM_ERROR")
            .WithMessage("A custom error occurred")
            .WithDetails("This is a detailed explanation of what went wrong")
            .AddMetadata("timestamp", DateTime.UtcNow)
            .AddMetadata("userId", 123)
            .Build();

        var result = Result<string>.Failed(error);

        Console.WriteLine($"Error Code: {result.Error?.Code}");
        Console.WriteLine($"Error Message: {result.Error?.Message}");
        Console.WriteLine($"Error Details: {result.Error?.Details}");
        Console.WriteLine($"Metadata Count: {result.Error?.Metadata?.Count}");

        // Build typed error
        var validationError = new ErrorBuilder()
            .WithCode("VAL_001")
            .WithMessage("Validation failed")
            .AddMetadata("field", "Email")
            .Build<ValidationError>();

        Console.WriteLine($"Typed error: {validationError.GetType().Name}");

        Console.WriteLine();
    }
    #endregion
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

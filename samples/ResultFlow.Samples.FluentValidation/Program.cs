using ResultFlow.FluentValidation.Extensions;
using ResultFlow.Samples.FluentValidation.Models;
using ResultFlow.Samples.FluentValidation.Validators;

namespace ResultFlow.Samples.FluentValidation;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== ResultFlow FluentValidation Integration Examples ===\n");

        // Example 1: Basic validation
        await BasicValidationExample();

        // Example 2: Complex validation with multiple rules
        await ComplexValidationExample();

        // Example 3: Validation in service layer
        await ServiceLayerExample();

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    static async Task BasicValidationExample()
    {
        Console.WriteLine("--- Example 1: Basic Validation ---");

        var validator = new CreateUserRequestValidator();

        // Valid user
        var validRequest = new CreateUserRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Age = 30
        };

        // Validate and convert to Result<T>
        var fluentValidationResult = await validator.ValidateAsync(validRequest);
        var validResult = fluentValidationResult.ToResult(validRequest);

        validResult.Match(
            onSuccess: user => Console.WriteLine($"✓ Valid user: {user.Name}"),
            onFailure: error => Console.WriteLine($"✗ Validation failed: {error.Message}")
        );

        // Invalid user
        var invalidRequest = new CreateUserRequest
        {
            Name = "",
            Email = "invalid-email",
            Age = 15
        };

        var fluentInvalidResult = await validator.ValidateAsync(invalidRequest);
        var invalidResult = fluentInvalidResult.ToResult(invalidRequest);

        invalidResult.Match(
            onSuccess: user => Console.WriteLine($"Valid user: {user.Name}"),
            onFailure: error =>
            {
                Console.WriteLine($"✗ Validation failed: {error.Message}");
                if (error.Metadata != null && error.Metadata.TryGetValue("errors", out var errorsObj))
                {
                    Console.WriteLine("  Validation errors:");
                    var errors = errorsObj as Dictionary<string, List<string>>;
                    foreach (var kvp in errors!)
                    {
                        Console.WriteLine($"    - {kvp.Key}: {string.Join(", ", kvp.Value)}");
                    }
                }
            }
        );

        Console.WriteLine();
    }

    static async Task ComplexValidationExample()
    {
        Console.WriteLine("--- Example 2: Complex Validation ---");

        var validator = new UpdateUserRequestValidator();

        var request = new UpdateUserRequest
        {
            Name = "A", // Too short
            Email = "test@example.com",
            Age = 150, // Too old
            Website = "not-a-url" // Invalid URL
        };

        var fluentResult = await validator.ValidateAsync(request);
        var result = fluentResult.ToResult(request);

        result.Match(
            onSuccess: user => Console.WriteLine($"✓ Valid: {user.Name}"),
            onFailure: error =>
            {
                Console.WriteLine($"✗ Validation failed: {error.Message}");
                Console.WriteLine($"Details: {error.Details}");
            }
        );

        Console.WriteLine();
    }

    static async Task ServiceLayerExample()
    {
        Console.WriteLine("--- Example 3: Service Layer Integration ---");

        var userService = new UserService();

        // Create valid user
        var validRequest = new CreateUserRequest
        {
            Name = "Alice Johnson",
            Email = "alice@example.com",
            Age = 28
        };

        var createResult = await userService.CreateUserAsync(validRequest);
        createResult.Match(
            onSuccess: user => Console.WriteLine($"✓ User created: ID={user.Id}, Name={user.Name}"),
            onFailure: error => Console.WriteLine($"✗ Failed: {error.Message}")
        );

        // Try to create duplicate
        var duplicateResult = await userService.CreateUserAsync(validRequest);
        duplicateResult.Match(
            onSuccess: user => Console.WriteLine($"User created: {user.Name}"),
            onFailure: error => Console.WriteLine($"✗ Expected conflict: {error.Message}")
        );

        // Create invalid user
        var invalidRequest = new CreateUserRequest
        {
            Name = "B",
            Email = "bad-email",
            Age = 10
        };

        var invalidResult = await userService.CreateUserAsync(invalidRequest);
        invalidResult.Match(
            onSuccess: user => Console.WriteLine($"User created: {user.Name}"),
            onFailure: error =>
            {
                Console.WriteLine($"✗ Validation failed: {error.Message}");
                if (error.Details != null)
                {
                    Console.WriteLine($"  Details: {error.Details}");
                }
            }
        );

        Console.WriteLine();
    }
}

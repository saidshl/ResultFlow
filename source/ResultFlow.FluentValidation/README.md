# ResultFlow.FluentValidation

FluentValidation integration for [ResultFlow](https://github.com/said1993/ResultFlow).

This package provides seamless integration between [FluentValidation](https://fluentvalidation.net/) and ResultFlow, allowing you to convert validation results into `Result<T>` objects with comprehensive error handling.

## ?? Installation

Install the NuGet package:

```bash
dotnet add package Shl.ResultFlow.FluentValidation --version 1.0.1-beta
```

Or via Package Manager:

```powershell
Install-Package Shl.ResultFlow.FluentValidation -Version 1.0.1-beta
```

## ?? Quick Start

```csharp
using FluentValidation;
using ResultFlow.Results;
using ResultFlow.FluentValidation.Extensions;

// Define your validator
public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be valid");

        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(18).WithMessage("User must be 18 or older");
    }
}

// Use the validator with ResultFlow
public class UserService
{
    private readonly UserValidator _validator;

    public async Task<Result<User>> CreateUserAsync(User user)
    {
        // Validate and convert to Result in one call
        return await _validator.ValidateAsync(user);
    }
}
```

## ? Features

- ? **Seamless Integration** - Convert FluentValidation results directly to `Result<T>`
- ? **Comprehensive Error Details** - Capture all validation errors with property-level grouping
- ? **Async Support** - Full async/await validation support
- ? **Type Safe** - Strongly typed validation with generic support
- ? **Rich Metadata** - Validation errors stored as metadata for API responses
- ? **Zero Configuration** - Works out of the box with existing validators

## ?? How It Works

The FluentValidation integration provides two extension methods:

### 1. ToResult<T> - Convert ValidationResult

```csharp
public static Result<T> ToResult<T>(
    this ValidationResult validationResult, 
    T? value = default)
```

Converts a FluentValidation `ValidationResult` to a `Result<T>`:
- **On Success** ? Returns `Result<T>.Success(value)`
- **On Failure** ? Returns `Result<T>.Failure(error)` with validation errors grouped by property

### 2. ValidateAsync<T> - Validate and Convert

```csharp
public static async Task<Result<T>> ValidateAsync<T>(
    this IValidator<T> validator, 
    T instance)
```

Validates an object and returns a `Result<T>` in a single call.

## ?? Usage Examples

### Basic Validation

```csharp
public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Must be a valid email address");

        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(18).WithMessage("Must be 18 or older");
    }
}

// Usage
var validator = new UserValidator();
var result = await validator.ValidateAsync(user);

if (result.IsOk)
{
    // User is valid, use result.Value
    Console.WriteLine($"User {result.Value.Name} is valid");
}
else
{
    // Validation failed, errors are in result.Error
    Console.WriteLine($"Validation failed: {result.Error.Message}");
    Console.WriteLine($"Details: {result.Error.Details}");
}
```

### Validation in Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly UserValidator _validator;

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        // Validate request
        var validationResult = await _validator.ValidateAsync(request);
        
        // Convert to Result and check
        if (!validationResult.IsOk)
            return validationResult.ToActionResult();

        // Process valid request
        var user = new User 
        { 
            Name = request.Name, 
            Email = request.Email 
        };
        
        var result = await _userService.CreateUserAsync(user);
        return result.ToActionResult();
    }
}
```

### Validation in Services

```csharp
public class UserService : IUserService
{
    private readonly UserValidator _validator;
    private readonly IUserRepository _repository;

    public async Task<Result<User>> CreateUserAsync(User user)
    {
        // Validate the user
        var validationResult = await _validator.ValidateAsync(user);
        
        if (!validationResult.IsOk)
            return validationResult;

        // Proceed with creation
        try
        {
            var createdUser = await _repository.AddAsync(user);
            return Result<User>.Success(createdUser);
        }
        catch (Exception ex)
        {
            return Result<User>.Failure(
                InternalServerError.FromException(ex)
            );
        }
    }
}
```

### Complex Validation Rules

```csharp
public class ProductValidator : AbstractValidator<Product>
{
    public ProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .Length(2, 100).WithMessage("Product name must be between 2 and 100 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero")
            .LessThanOrEqualTo(999999).WithMessage("Price cannot exceed 999,999");

        RuleFor(x => x.Description)
            .NotEmpty().When(x => x.Price > 100)
            .WithMessage("Description is required for products over 100");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .Must(ValidateCategory).WithMessage("Invalid category");

        RuleForEach(x => x.Tags)
            .NotEmpty().WithMessage("Tags cannot be empty");
    }

    private bool ValidateCategory(string category)
        => !string.IsNullOrEmpty(category) && category.Length <= 50;
}

// Usage
var validator = new ProductValidator();
var result = await validator.ValidateAsync(product);

result.Match(
    onSuccess: validProduct => Console.WriteLine($"Product {validProduct.Name} is valid"),
    onFailure: error => Console.WriteLine($"Validation error: {error.Message}")
);
```

### Chaining with Other Operations

```csharp
public class OrderService
{
    private readonly OrderValidator _validator;
    private readonly IOrderRepository _repository;

    public async Task<Result<Order>> CreateOrderAsync(Order order)
    {
        return await _validator.ValidateAsync(order)
            .Then(validOrder => ValidateInventoryAsync(validOrder))
            .Then(validOrder => ValidatePricingAsync(validOrder))
            .Then(validOrder => SaveOrderAsync(validOrder));
    }

    private async Task<Result<Order>> ValidateInventoryAsync(Order order)
    {
        foreach (var item in order.Items)
        {
            var stock = await _repository.GetStockAsync(item.ProductId);
            if (stock < item.Quantity)
                return Result<Order>.Failure(
                    ValidationError.WithDefaults(
                        "Insufficient inventory",
                        details: $"Product {item.ProductId} has only {stock} units available"
                    )
                );
        }
        
        return Result<Order>.Success(order);
    }

    private async Task<Result<Order>> ValidatePricingAsync(Order order)
    {
        // Additional pricing validation
        return Result<Order>.Success(order);
    }

    private async Task<Result<Order>> SaveOrderAsync(Order order)
    {
        try
        {
            var saved = await _repository.AddAsync(order);
            return Result<Order>.Success(saved);
        }
        catch (Exception ex)
        {
            return Result<Order>.Failure(
                InternalServerError.FromException(ex)
            );
        }
    }
}
```

## ?? Error Response Format

When validation fails, the error response includes detailed information:

```json
{
  "code": "VALIDATION_FAILED",
  "message": "Validation failed",
  "details": "{\"Name\":[\"Name is required\"],\"Email\":[\"Email is required\",\"Must be a valid email address\"],\"Age\":[\"Must be 18 or older\"]}",
  "metadata": {
    "errors": {
      "Name": ["Name is required"],
      "Email": ["Email is required", "Must be a valid email address"],
      "Age": ["Must be 18 or older"]
    }
  }
}
```

## ?? Real-World Example: Complete Service

```csharp
using FluentValidation;
using ResultFlow.Results;
using ResultFlow.FluentValidation.Extensions;

// Domain Model
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public List<string> Tags { get; set; } = new();
}

// Validator
public class ProductValidator : AbstractValidator<Product>
{
    public ProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .Length(3, 100).WithMessage("Product name must be between 3 and 100 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero")
            .LessThanOrEqualTo(999999).WithMessage("Price cannot exceed 999,999");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MinimumLength(10).WithMessage("Description must be at least 10 characters");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .Length(2, 50).WithMessage("Category must be between 2 and 50 characters");

        RuleForEach(x => x.Tags)
            .NotEmpty().WithMessage("Tags cannot be empty")
            .Length(2, 20).WithMessage("Each tag must be between 2 and 20 characters");
    }
}

// Repository Interface
public interface IProductRepository
{
    Task<Product> AddAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task<Product> GetByIdAsync(int id);
    Task DeleteAsync(int id);
}

// Service
public class ProductService : IProductService
{
    private readonly ProductValidator _validator;
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        ProductValidator validator,
        IProductRepository repository,
        ILogger<ProductService> logger)
    {
        _validator = validator;
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<Product>> CreateProductAsync(Product product)
    {
        _logger.LogInformation("Creating product: {ProductName}", product.Name);

        return await _validator.ValidateAsync(product)
            .Bind(validProduct => SaveProductAsync(validProduct))
            .Tap(savedProduct => _logger.LogInformation(
                "Product created successfully: {ProductId}", savedProduct.Id))
            .TapError(error => _logger.LogError(
                "Failed to create product: {ErrorMessage}", error.Message));
    }

    public async Task<Result<Product>> UpdateProductAsync(int id, Product product)
    {
        _logger.LogInformation("Updating product: {ProductId}", id);

        var existingProduct = await _repository.GetByIdAsync(id);
        if (existingProduct == null)
            return Result<Product>.Failure(
                NotFoundError.ByIdentifier("Product", id)
            );

        product.Id = id;

        return await _validator.ValidateAsync(product)
            .Bind(validProduct => UpdateProductInDatabaseAsync(validProduct))
            .Tap(updatedProduct => _logger.LogInformation(
                "Product updated successfully: {ProductId}", updatedProduct.Id))
            .TapError(error => _logger.LogError(
                "Failed to update product: {ErrorMessage}", error.Message));
    }

    private async Task<Result<Product>> SaveProductAsync(Product product)
    {
        try
        {
            var savedProduct = await _repository.AddAsync(product);
            return Result<Product>.Success(savedProduct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error while creating product");
            return Result<Product>.Failure(
                InternalServerError.ForOperation("CreateProduct", ex)
            );
        }
    }

    private async Task<Result<Product>> UpdateProductInDatabaseAsync(Product product)
    {
        try
        {
            var updatedProduct = await _repository.UpdateAsync(product);
            return Result<Product>.Success(updatedProduct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error while updating product");
            return Result<Product>.Failure(
                InternalServerError.ForOperation("UpdateProduct", ex)
            );
        }
    }
}

// Controller
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _service;

    public ProductsController(ProductService service) => _service = service;

    [HttpPost]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateProduct(CreateProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            Price = request.Price,
            Description = request.Description,
            Category = request.Category,
            Tags = request.Tags
        };

        var result = await _service.CreateProductAsync(product);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductRequest request)
    {
        var product = new Product
        {
            Id = id,
            Name = request.Name,
            Price = request.Price,
            Description = request.Description,
            Category = request.Category,
            Tags = request.Tags
        };

        var result = await _service.UpdateProductAsync(id, product);
        return result.ToActionResult();
    }
}
```

## ?? Integration with ASP.NET Core

When combined with `Shl.ResultFlow.AspNetCore`, validation errors automatically map to HTTP 422 responses:

```csharp
[HttpPost]
public async Task<IActionResult> CreateUser(CreateUserRequest request)
{
    var user = new User { Name = request.Name, Email = request.Email };
    var result = await _userValidator.ValidateAsync(user);
    
    // If validation fails, automatically returns 422 with detailed error info
    return result.ToActionResult();
}
```

## ?? API Reference

### ToResult<T>

```csharp
/// <summary>
/// Converts FluentValidation results to Result{T}
/// </summary>
/// <typeparam name="T">The type of the value being validated.</typeparam>
/// <param name="validationResult">The FluentValidation result to convert.</param>
/// <param name="value">The value associated with the validation result.</param>
/// <returns>A successful Result{T} if validation passed, otherwise a failed Result{T}.</returns>
public static Result<T> ToResult<T>(
    this ValidationResult validationResult, 
    T? value = default)
```

### ValidateAsync<T>

```csharp
/// <summary>
/// Validates an object and returns Result{T}
/// </summary>
/// <typeparam name="T">The type of the object being validated.</typeparam>
/// <param name="validator">The FluentValidation validator to use.</param>
/// <param name="instance">The instance to validate.</param>
/// <returns>A successful Result{T} if validation passed, otherwise a failed Result{T}.</returns>
public static async Task<Result<T>> ValidateAsync<T>(
    this IValidator<T> validator, 
    T instance)
```

## ?? Testing with Validation

```csharp
[TestClass]
public class ProductValidatorTests
{
    private ProductValidator _validator;

    [TestInitialize]
    public void Setup() => _validator = new ProductValidator();

    [TestMethod]
    public async Task CreateProduct_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            Price = 99.99m,
            Description = "This is a test product",
            Category = "Electronics",
            Tags = new List<string> { "test", "product" }
        };

        // Act
        var result = await _validator.ValidateAsync(product);

        // Assert
        Assert.IsTrue(result.IsOk);
        Assert.AreEqual(product, result.Value);
    }

    [TestMethod]
    public async Task CreateProduct_WithInvalidPrice_ReturnsFailed()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            Price = 0,  // Invalid
            Description = "This is a test product",
            Category = "Electronics",
            Tags = new List<string> { "test", "product" }
        };

        // Act
        var result = await _validator.ValidateAsync(product);

        // Assert
        Assert.IsFalse(result.IsOk);
        Assert.IsTrue(result.Error.Message.Contains("Validation failed"));
    }
}
```

## ?? Contributing

Contributions are welcome! You can:
- ?? Report bugs by creating an [Issue](https://github.com/said1993/ResultFlow/issues)
- ?? Suggest features in [Discussions](https://github.com/said1993/ResultFlow/discussions)
- ?? Submit Pull Requests with improvements

## ?? Support

Need help?
- ? Create an [Issue](https://github.com/said1993/ResultFlow/issues)
- ?? Start a [Discussion](https://github.com/said1993/ResultFlow/discussions)
- ?? Visit the [Repository](https://github.com/said1993/ResultFlow)

## ?? Related Packages

- [Shl.ResultFlow](https://www.nuget.org/packages/Shl.ResultFlow/) - Core library
- [Shl.ResultFlow.AspNetCore](https://www.nuget.org/packages/Shl.ResultFlow.AspNetCore/) - ASP.NET Core extensions
- Shl.ResultFlow.Logging - Structured logging integration (coming soon)

## ?? Workflow Example

```
User Input
    ?
Validator (FluentValidation)
    ?
ValidateAsync() Extension
    ?
Result<T> (Success or Failure)
    ?
Service Logic (if validation passed)
    ?
ToActionResult() Extension (if using ASP.NET Core)
    ?
HTTP Response (200, 400, 422, 500, etc.)
```

---

**Made with ?? by [said1993](https://github.com/said1993)**

Updated: 2025-11-23 19:08:17 UTC

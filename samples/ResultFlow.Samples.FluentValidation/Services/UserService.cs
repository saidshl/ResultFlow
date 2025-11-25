using FluentValidation;
using ResultFlow.Errors;
using ResultFlow.Extensions;
using ResultFlow.FluentValidation.Extensions;
using ResultFlow.Results;
using ResultFlow.Samples.FluentValidation.Models;
using ResultFlow.Samples.FluentValidation.Validators;
using System.Collections.Concurrent;

namespace ResultFlow.Samples.FluentValidation;

public class UserService
{
    private readonly ConcurrentDictionary<int, User> _users = new();
    private readonly IValidator<CreateUserRequest> _createValidator;
    private readonly IValidator<UpdateUserRequest> _updateValidator;
    private int _nextId = 1;

    public UserService()
    {
        _createValidator = new CreateUserRequestValidator();
        _updateValidator = new UpdateUserRequestValidator();
    }

    public async Task<Result<User>> CreateUserAsync(CreateUserRequest request)
    {
        // Validate using FluentValidation - automatically converts to Result
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return validationResult.ToResult<User>();
        }

        // Check for duplicate email
        if (_users.Values.Any(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<User>.Failed(
                ConflictError.ForDuplicateResource("User", request.Email)
            );
        }

        // Create user
        var user = new User
        {
            Id = _nextId++,
            Name = request.Name,
            Email = request.Email,
            Age = request.Age,
            CreatedAt = DateTime.UtcNow
        };

        _users.TryAdd(user.Id, user);

        return Result<User>.Ok(user);
    }

    public async Task<Result<User>> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        // Railway-oriented approach with validation
        return await Task.FromResult(GetUser(id))
            .BindAsync(async user =>
            {
                // Validate request
                var validationResult = await _updateValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return validationResult.ToResult<User>();
                }

                // Check email conflict
                if (!user.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase))
                {
                    if (_users.Values.Any(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
                    {
                        return Result<User>.Failed(
                            ConflictError.ForDuplicateResource("User", request.Email)
                        );
                    }
                }

                // Update user
                user.Name = request.Name;
                user.Email = request.Email;
                user.Age = request.Age;
                user.Website = request.Website;

                return Result<User>.Ok(user);
            });
    }

    private Result<User> GetUser(int id)
    {
        if (_users.TryGetValue(id, out var user))
        {
            return Result<User>.Ok(user);
        }

        return Result<User>.Failed(NotFoundError.ByIdentifier("User", id));
    }
}

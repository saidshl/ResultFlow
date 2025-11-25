using ResultFlow.Extensions;

namespace ResultFlow.Samples.WebApi.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<User>> GetUserAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Result<IEnumerable<User>>> GetAllUsersAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Result<User>> CreateUserAsync(CreateUserRequest request)
    {
        // Validate request
        var validationResult = ValidateCreateRequest(request);
        if (validationResult.HasError)
        {
            return Result<User>.Failed(validationResult.Error!);
        }

        // Check for duplicate email
        var existsResult = await _repository.ExistsAsync(request.Email);
        if (existsResult.IsOk && existsResult.Value)
        {
            return ConflictError.ForDuplicateResource("User", request.Email);
        }

        // Create user
        var user = new User
        {
            Name = request.Name,
            Email = request.Email
        };

        return await _repository.CreateAsync(user);
    }

    public async Task<Result<User>> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        // Validate request
        var validationResult = ValidateUpdateRequest(request);
        if (validationResult.HasError)
        {
            return Result<User>.Failed(validationResult.Error!);
        }

        // Get existing user - Railway-oriented approach
        return await _repository.GetByIdAsync(id)
            .BindAsync(async existingUser =>
            {
                // Check email conflict (if email changed)
                if (!existingUser.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var existsResult = await _repository.ExistsAsync(request.Email);
                    if (existsResult.IsOk && existsResult.Value)
                    {
                        return Result<User>.Failed(
                            ConflictError.ForDuplicateResource("User", request.Email)
                        );
                    }
                }

                // Update properties
                existingUser.Name = request.Name;
                existingUser.Email = request.Email;

                return await _repository.UpdateAsync(existingUser);
            });
    }

    public async Task<VoidResult> DeleteUserAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    private VoidResult ValidateCreateRequest(CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return VoidResult.Failed(
                ValidationError.ForField("Name", "Name is required")
            );
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return VoidResult.Failed(
                ValidationError.ForField("Email", "Email is required")
            );
        }

        if (!IsValidEmail(request.Email))
        {
            return VoidResult.Failed(
                ValidationError.ForField("Email", "Invalid email format")
            );
        }

        return VoidResult.Ok();
    }

    private VoidResult ValidateUpdateRequest(UpdateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return VoidResult.Failed(
                ValidationError.ForField("Name", "Name is required")
            );
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return VoidResult.Failed(
                ValidationError.ForField("Email", "Email is required")
            );
        }

        if (!IsValidEmail(request.Email))
        {
            return VoidResult.Failed(
                ValidationError.ForField("Email", "Invalid email format")
            );
        }

        return VoidResult.Ok();
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

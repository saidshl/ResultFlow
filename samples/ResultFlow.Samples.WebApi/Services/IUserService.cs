using ResultFlow.Results;
using ResultFlow.Samples.WebApi.Models;

namespace ResultFlow.Samples.WebApi.Services;

public interface IUserService
{
    Task<Result<User>> GetUserAsync(int id);
    Task<Result<IEnumerable<User>>> GetAllUsersAsync();
    Task<Result<User>> CreateUserAsync(CreateUserRequest request);
    Task<Result<User>> UpdateUserAsync(int id, UpdateUserRequest request);
    Task<VoidResult> DeleteUserAsync(int id);
}

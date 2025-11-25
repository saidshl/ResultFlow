using ResultFlow.Results;
using ResultFlow.Samples.WebApi.Models;

namespace ResultFlow.Samples.WebApi.Repositories;

public interface IUserRepository
{
    Task<Result<User>> GetByIdAsync(int id);
    Task<Result<IEnumerable<User>>> GetAllAsync();
    Task<Result<User>> CreateAsync(User user);
    Task<Result<User>> UpdateAsync(User user);
    Task<VoidResult> DeleteAsync(int id);
    Task<Result<bool>> ExistsAsync(string email);
}

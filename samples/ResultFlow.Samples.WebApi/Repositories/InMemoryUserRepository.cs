using ResultFlow.Errors;
using ResultFlow.Results;
using ResultFlow.Samples.WebApi.Models;
using System.Collections.Concurrent;

namespace ResultFlow.Samples.WebApi.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<int, User> _users = new();
    private int _nextId = 1;

    public InMemoryUserRepository()
    {
        // Seed some data
        _users.TryAdd(1, new User { Id = 1, Name = "John Doe", Email = "john@example.com", CreatedAt = DateTime.UtcNow.AddDays(-10) });
        _users.TryAdd(2, new User { Id = 2, Name = "Jane Smith", Email = "jane@example.com", CreatedAt = DateTime.UtcNow.AddDays(-5) });
        _nextId = 3;
    }

    public Task<Result<User>> GetByIdAsync(int id)
    {
        if (_users.TryGetValue(id, out var user))
        {
            return Task.FromResult(Result<User>.Ok(user));
        }

        return Task.FromResult(
            Result<User>.Failed(NotFoundError.ByIdentifier("User", id))
        );
    }

    public Task<Result<IEnumerable<User>>> GetAllAsync()
    {
        var users = _users.Values.OrderBy(u => u.Id).ToList();
        return Task.FromResult(Result<IEnumerable<User>>.Ok(users));
    }

    public Task<Result<User>> CreateAsync(User user)
    {
        user.Id = _nextId++;
        user.CreatedAt = DateTime.UtcNow;

        if (!_users.TryAdd(user.Id, user))
        {
            return Task.FromResult(
                Result<User>.Failed(
                    InternalServerError.FromException(
                        new InvalidOperationException("Failed to add user to repository")
                    )
                )
            );
        }

        return Task.FromResult(Result<User>.Ok(user));
    }

    public Task<Result<User>> UpdateAsync(User user)
    {
        if (!_users.ContainsKey(user.Id))
        {
            return Task.FromResult(
                Result<User>.Failed(NotFoundError.ByIdentifier("User", user.Id))
            );
        }

        _users[user.Id] = user;
        return Task.FromResult(Result<User>.Ok(user));
    }

    public Task<VoidResult> DeleteAsync(int id)
    {
        if (!_users.TryRemove(id, out _))
        {
            return Task.FromResult(
                VoidResult.Failed(NotFoundError.ByIdentifier("User", id))
            );
        }

        return Task.FromResult(VoidResult.Ok());
    }

    public Task<Result<bool>> ExistsAsync(string email)
    {
        var exists = _users.Values.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(Result<bool>.Ok(exists));
    }
}

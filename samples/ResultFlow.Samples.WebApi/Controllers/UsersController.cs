using ResultFlow.Extensions;
using ResultFlow.Extensions.AspNetCore;

namespace ResultFlow.Samples.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    /// <returns>List of users</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<User>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Getting all users");

        // ResultFlow automatically converts Result to IActionResult
        return await _userService.GetAllUsersAsync()
            .ToActionResultAsync();
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation("Getting user with ID: {UserId}", id);

        // Automatic error-to-HTTP status mapping:
        // - NotFoundError ? 404
        // - ValidationError ? 422
        // - BadRequestError ? 400
        // - UnauthorizedError ? 401
        // - ForbiddenError ? 403
        // - ConflictError ? 409
        // - InternalServerError ? 500
        return await _userService.GetUserAsync(id)
            .ToActionResultAsync();
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="request">User creation data</param>
    /// <returns>Created user</returns>
    [HttpPost]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        _logger.LogInformation("Creating new user with email: {Email}", request.Email);

        return await _userService.CreateUserAsync(request)
            .TapAsync(async user =>
            {
                // Side effect: Log success
                _logger.LogInformation("User created successfully: {UserId}", user?.Id);
                await Task.CompletedTask;
            })
            .ToActionResultAsync();
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">User update data</param>
    /// <returns>Updated user</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
    {
        _logger.LogInformation("Updating user with ID: {UserId}", id);

        return await _userService.UpdateUserAsync(id, request)
            .TapAsync(async user =>
            {
                _logger.LogInformation("User updated successfully: {UserId}", user?.Id);
                await Task.CompletedTask;
            })
            .ToActionResultAsync();
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Deleting user with ID: {UserId}", id);

        // VoidResult also converts automatically
        return await _userService.DeleteUserAsync(id)
            .TapAsync(async () =>
            {
                _logger.LogInformation("User deleted successfully: {UserId}", id);
                await Task.CompletedTask;
            })
            .ToActionResultAsync();
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClothsPosAPI.DTOs;
using ClothsPosAPI.Services;
using System.Text.Json;

namespace ClothsPosAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetUsers([FromQuery] string? roleId, [FromQuery] int page = 1, [FromQuery] int limit = 100)
    {
        try
        {
            var users = await _userService.GetAllUsersAsync(roleId, page, limit);
            var result = users.Select(u => MapToDto(u));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users");
            return StatusCode(500, new { message = "Error fetching users", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetUser(string id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(MapToDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user");
            return StatusCode(500, new { message = "Error fetching user", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateUser([FromBody] CreateUserDto userDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    );
                return BadRequest(new { message = "Validation failed", errors = errors });
            }

            var user = await _userService.CreateUserAsync(userDto);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, MapToDto(user));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, new { message = "Error creating user", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<object>> UpdateUser(string id, [FromBody] UpdateUserDto userDto)
    {
        try
        {
            if (id != userDto.Id)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.UpdateUserAsync(id, userDto);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(MapToDto(user));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user");
            return StatusCode(500, new { message = "Error updating user", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        try
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
            {
                return NotFound(new { message = "User not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user");
            return StatusCode(500, new { message = "Error deleting user", error = ex.Message });
        }
    }

    [HttpPost("{id}/password")]
    public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordDto dto)
    {
        try
        {
            var result = await _userService.ChangePasswordAsync(id, dto.OldPassword, dto.NewPassword);
            if (!result)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(new { message = "Password changed successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return StatusCode(500, new { message = "Error changing password", error = ex.Message });
        }
    }

    private object MapToDto(Models.User user)
    {
        var permissions = JsonSerializer.Deserialize<string[]>(user.Permissions) ?? Array.Empty<string>();

        return new
        {
            id = user.Id,
            username = user.Username,
            email = user.Email,
            roleId = user.RoleId,
            roleName = user.Role?.Name ?? "Unknown",
            permissions = permissions,
            isActive = user.IsActive,
            createdAt = user.CreatedAt.ToString("yyyy-MM-dd"),
            updatedAt = user.UpdatedAt.ToString("yyyy-MM-dd")
        };
    }
}

public class ChangePasswordDto
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}



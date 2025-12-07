using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClothsPosAPI.DTOs;
using ClothsPosAPI.Services;
using System.Text.Json;

namespace ClothsPosAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RolesController> _logger;

    public RolesController(IRoleService roleService, ILogger<RolesController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous] // Allow access without auth for bootstrapping
    public async Task<ActionResult<IEnumerable<object>>> GetRoles()
    {
        try
        {
            var roles = await _roleService.GetAllRolesAsync();
            var result = roles.Select(r => MapToDto(r));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching roles");
            return StatusCode(500, new { message = "Error fetching roles", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetRole(string id)
    {
        try
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }

            return Ok(MapToDto(role));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching role");
            return StatusCode(500, new { message = "Error fetching role", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateRole([FromBody] CreateRoleDto roleDto)
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

            var role = await _roleService.CreateRoleAsync(roleDto);
            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, MapToDto(role));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return StatusCode(500, new { message = "Error creating role", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<object>> UpdateRole(string id, [FromBody] UpdateRoleDto roleDto)
    {
        try
        {
            if (id != roleDto.Id)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

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

            var role = await _roleService.UpdateRoleAsync(id, roleDto);
            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }

            return Ok(MapToDto(role));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role");
            return StatusCode(500, new { message = "Error updating role", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(string id)
    {
        try
        {
            var result = await _roleService.DeleteRoleAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Role not found" });
            }

            return Ok(new { message = "Role deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role");
            return StatusCode(500, new { message = "Error deleting role", error = ex.Message });
        }
    }

    private object MapToDto(Models.Role role)
    {
        var permissions = JsonSerializer.Deserialize<string[]>(role.Permissions) ?? Array.Empty<string>();

        return new
        {
            id = role.Id,
            name = role.Name,
            description = role.Description,
            permissions = permissions,
            isActive = role.IsActive,
            isSystemRole = role.IsSystemRole,
            displayOrder = role.DisplayOrder,
            createdAt = role.CreatedAt.ToString("yyyy-MM-dd"),
            updatedAt = role.UpdatedAt.ToString("yyyy-MM-dd"),
            userCount = role.Users?.Count ?? 0
        };
    }
}


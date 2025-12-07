using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClothsPosAPI.DTOs;
using ClothsPosAPI.Services;
using ClothsPosAPI.Data;
using ClothsPosAPI.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Text.Json;

namespace ClothsPosAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ApplicationDbContext context, ILogger<AuthController> logger)
    {
        _authService = authService;
        _context = context;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(loginDto);
            if (result == null)
            {
                return Unauthorized(new { message = "Invalid credentials or access denied. Only Admin users can access the web portal." });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { message = "Error during login", error = ex.Message });
        }
    }

    [HttpPost("mobile-login")]
    public async Task<ActionResult<LoginResponseDto>> MobileLogin([FromBody] LoginDto loginDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.MobileLoginAsync(loginDto);
            if (result == null)
            {
                return Unauthorized(new { message = "Invalid credentials. Please check your username and password." });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during mobile login");
            return StatusCode(500, new { message = "Error during login", error = ex.Message });
        }
    }

    /// <summary>
    /// Bootstrap endpoint to create the first admin user
    /// Only works if no admin user exists in the database
    /// </summary>
    [HttpPost("bootstrap")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> BootstrapAdmin([FromBody] BootstrapAdminDto dto)
    {
        try
        {
            // Check if any admin user already exists
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null)
            {
                return BadRequest(new { message = "Admin role not found. Please seed roles first." });
            }

            var existingAdmin = await _context.Users
                .FirstOrDefaultAsync(u => u.RoleId == adminRole.Id && u.IsActive);

            if (existingAdmin != null)
            {
                return BadRequest(new { message = "Admin user already exists. Cannot bootstrap." });
            }

            // Validate input
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { message = "Username and password are required" });
            }

            if (dto.Username.Length < 3 || dto.Username.Length > 50)
            {
                return BadRequest(new { message = "Username must be between 3 and 50 characters" });
            }

            if (dto.Password.Length < 4 || dto.Password.Length > 20)
            {
                return BadRequest(new { message = "Password must be between 4 and 20 characters" });
            }

            // Create admin user
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            
            var adminUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = dto.Username,
                Email = dto.Email ?? $"{dto.Username}@clothpos.test",
                RoleId = adminRole.Id,
                Passcode = hashedPassword,
                Permissions = "[\"all\"]",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Admin user created successfully",
                username = adminUser.Username,
                email = adminUser.Email
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bootstrap");
            return StatusCode(500, new { message = "Error during bootstrap", error = ex.Message });
        }
    }
}

public class BootstrapAdminDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Email { get; set; }
}



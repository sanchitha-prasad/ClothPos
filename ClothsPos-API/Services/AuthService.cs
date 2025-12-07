using Microsoft.EntityFrameworkCore;
using ClothsPosAPI.Data;
using ClothsPosAPI.DTOs;
using ClothsPosAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using BCrypt.Net;

namespace ClothsPosAPI.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto)
    {
        // Find user by username (primary) or email (fallback for backward compatibility)
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => 
                (!string.IsNullOrEmpty(u.Username) && (u.Username == loginDto.Username || u.Username == loginDto.Username.ToLower())) ||
                u.Email == loginDto.Username ||
                u.Email == loginDto.Username.ToLower());

        if (user == null || !user.IsActive)
        {
            return null;
        }

        // Check if user has Admin role - only Admin users can login to web portal
        if (user.Role == null || user.Role.Name != "Admin")
        {
            return null; // Reject non-admin users for web portal
        }

        // Verify password
        if (string.IsNullOrEmpty(user.Passcode))
        {
            return null;
        }

        // For all users (including admin), verify with BCrypt if it's a BCrypt hash
        // Otherwise check plain text (for backward compatibility)
        bool passwordValid = false;
        
        if (user.Passcode.StartsWith("$2a$") || user.Passcode.StartsWith("$2b$") || user.Passcode.StartsWith("$2y$"))
        {
            // BCrypt hash detected
            passwordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Passcode);
        }
        else
        {
            // Plain text password (for backward compatibility)
            passwordValid = user.Passcode == loginDto.Password;
        }

        if (!passwordValid)
        {
            return null;
        }

        var permissions = JsonSerializer.Deserialize<string[]>(user.Permissions) ?? Array.Empty<string>();
        
        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            RoleId = user.RoleId,
            RoleName = user.Role?.Name,
            Permissions = permissions,
            IsActive = user.IsActive
        };

        var jwtToken = GenerateJwtToken(userDto);
        return new LoginResponseDto { Token = jwtToken, User = userDto };
    }

    // Mobile login - allows any active user (not just Admin)
    public async Task<LoginResponseDto?> MobileLoginAsync(LoginDto loginDto)
    {
        // Find user by username (primary) or email (fallback for backward compatibility)
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => 
                (!string.IsNullOrEmpty(u.Username) && (u.Username == loginDto.Username || u.Username == loginDto.Username.ToLower())) ||
                u.Email == loginDto.Username ||
                u.Email == loginDto.Username.ToLower());

        if (user == null || !user.IsActive)
        {
            return null;
        }

        // Mobile allows any active user (no role restriction)
        // Verify password
        if (string.IsNullOrEmpty(user.Passcode))
        {
            return null;
        }

        // For all users (including admin), verify with BCrypt if it's a BCrypt hash
        // Otherwise check plain text (for backward compatibility)
        bool passwordValid = false;
        
        if (user.Passcode.StartsWith("$2a$") || user.Passcode.StartsWith("$2b$") || user.Passcode.StartsWith("$2y$"))
        {
            // BCrypt hash detected
            passwordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Passcode);
        }
        else
        {
            // Plain text password (for backward compatibility)
            passwordValid = user.Passcode == loginDto.Password;
        }

        if (!passwordValid)
        {
            return null;
        }

        var permissions = JsonSerializer.Deserialize<string[]>(user.Permissions) ?? Array.Empty<string>();
        
        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            RoleId = user.RoleId,
            RoleName = user.Role?.Name,
            Permissions = permissions,
            IsActive = user.IsActive
        };

        var jwtToken = GenerateJwtToken(userDto);
        return new LoginResponseDto { Token = jwtToken, User = userDto };
    }

    private string GenerateJwtToken(UserDto user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "ClothsPosAPI";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "ClothsPosClient";
        var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id ?? ""),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.RoleName ?? ""),
            new Claim("roleId", user.RoleId ?? ""),
            new Claim("permissions", JsonSerializer.Serialize(user.Permissions))
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}


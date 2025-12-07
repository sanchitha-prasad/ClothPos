using System.ComponentModel.DataAnnotations;
using ClothsPosAPI.Models;

namespace ClothsPosAPI.DTOs;

public class UserDto
{
    public string? Id { get; set; }
    
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    public string Username { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Role is required")]
    public string RoleId { get; set; } = string.Empty;
    
    public string? RoleName { get; set; } // For display purposes
    
    [StringLength(20, MinimumLength = 4, ErrorMessage = "Passcode must be between 4 and 20 characters")]
    public string? Passcode { get; set; }
    
    public string[] Permissions { get; set; } = Array.Empty<string>();
    
    public bool IsActive { get; set; } = true;
}

public class CreateUserDto : UserDto
{
    [Required(ErrorMessage = "Passcode is required for new users")]
    [StringLength(20, MinimumLength = 4, ErrorMessage = "Passcode must be between 4 and 20 characters")]
    public new string Passcode { get; set; } = string.Empty;
}

public class UpdateUserDto : UserDto
{
    [Required]
    public new string Id { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}


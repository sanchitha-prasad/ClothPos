using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClothsPosAPI.Models;

public class User
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty; // Unique username for login
    
    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(450)]
    public string RoleId { get; set; } = string.Empty; // Foreign key to Role table
    
    [ForeignKey("RoleId")]
    public virtual Role? Role { get; set; }
    
    public string? Passcode { get; set; }
    
    public string Permissions { get; set; } = "[]"; // JSON string array
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}



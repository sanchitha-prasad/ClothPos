using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClothsPosAPI.Models;

public class Role
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty; // e.g., "Admin", "Cashier", "Mobile Staff"
    
    [StringLength(200)]
    public string? Description { get; set; }
    
    public string Permissions { get; set; } = "[]"; // JSON string array of default permissions
    
    public bool IsActive { get; set; } = true;
    
    public bool IsSystemRole { get; set; } = false; // System roles cannot be deleted
    
    public int DisplayOrder { get; set; } = 0; // For sorting
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}


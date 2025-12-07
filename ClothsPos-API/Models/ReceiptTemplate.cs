using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClothsPosAPI.Models;

public class ReceiptTemplate
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public string Content { get; set; } = string.Empty; // HTML template content or JSON structure
    
    public string? Structure { get; set; } // JSON structure for visual builder (optional)
    
    public bool IsDefault { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Template variables that can be used: {{shopName}}, {{date}}, {{total}}, {{items}}, etc.
}


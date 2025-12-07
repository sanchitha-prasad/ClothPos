using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClothsPosAPI.Models;

public class EmailTemplate
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty; // Email subject template
    
    [Required]
    public string Body { get; set; } = string.Empty; // HTML email body template or JSON structure
    
    public string? Structure { get; set; } // JSON structure for visual builder (optional)
    
    [StringLength(50)]
    public string Type { get; set; } = "general"; // general, receipt, invoice, notification, etc.
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Template variables that can be used: {{customerName}}, {{orderNumber}}, {{total}}, {{items}}, etc.
}


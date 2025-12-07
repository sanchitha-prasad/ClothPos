using System.ComponentModel.DataAnnotations;

namespace ClothsPosAPI.DTOs;

public class ReceiptTemplateDto
{
    public string? Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public string? Structure { get; set; } // JSON structure for visual builder
    
    public bool IsDefault { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
}


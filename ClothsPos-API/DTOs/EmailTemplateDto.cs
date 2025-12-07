using System.ComponentModel.DataAnnotations;

namespace ClothsPosAPI.DTOs;

public class EmailTemplateDto
{
    public string? Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;
    
    [Required]
    public string Body { get; set; } = string.Empty;
    
    public string? Structure { get; set; } // JSON structure for visual builder
    
    [StringLength(50)]
    public string Type { get; set; } = "general";
    
    public bool IsActive { get; set; } = true;
}


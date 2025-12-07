using System.ComponentModel.DataAnnotations;

namespace ClothsPosAPI.DTOs;

public class CategoryDto
{
    public string? Id { get; set; }
    
    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Description must be at most 500 characters")]
    public string? Description { get; set; }
}

public class CreateCategoryDto : CategoryDto
{
}

public class UpdateCategoryDto : CategoryDto
{
    [Required]
    public new string Id { get; set; } = string.Empty;
}


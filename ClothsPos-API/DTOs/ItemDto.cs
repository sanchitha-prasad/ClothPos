using System.ComponentModel.DataAnnotations;

namespace ClothsPosAPI.DTOs;

public class ItemDto
{
    public string? Id { get; set; }
    
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 200 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Category is required")]
    public string CategoryId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "SKU is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "SKU must be between 3 and 50 characters")]
    public string SKU { get; set; } = string.Empty;
    
    [StringLength(100, ErrorMessage = "Brand must be at most 100 characters")]
    public string? Brand { get; set; }
    
    [Required(ErrorMessage = "Price is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
    public decimal Price { get; set; }
    
    [Required(ErrorMessage = "Stock is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Stock must be greater than or equal to 0")]
    public decimal Stock { get; set; }
    
    public string? Note { get; set; }
    
    public string[]? Images { get; set; }
    
    [StringLength(50, ErrorMessage = "Barcode must be at most 50 characters")]
    public string? Barcode { get; set; }
}

public class CreateItemDto : ItemDto
{
}

public class UpdateItemDto : ItemDto
{
    [Required]
    public new string Id { get; set; } = string.Empty;
}


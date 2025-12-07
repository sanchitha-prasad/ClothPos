using System.ComponentModel.DataAnnotations;

namespace ClothsPosAPI.DTOs;

public class CreateSaleItemDto
{
    [Required]
    public string ItemId { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
    public decimal Price { get; set; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Total must be greater than or equal to 0")]
    public decimal Total { get; set; }
    
    // SaleId is not required - backend will set it automatically
}

public class CreateSaleDto
{
    [Required]
    public DateTime Date { get; set; } = DateTime.UtcNow;
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Subtotal must be greater than or equal to 0")]
    public decimal Subtotal { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Tax must be greater than or equal to 0")]
    public decimal Tax { get; set; } = 0;
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Total must be greater than or equal to 0")]
    public decimal Total { get; set; }
    
    [Required]
    [StringLength(50)]
    public string PaymentMethod { get; set; } = "cash";
    
    [StringLength(20)]
    public string Status { get; set; } = "completed";
    
    [StringLength(200)]
    public string? CustomerName { get; set; }
    
    [Required]
    public string CashierId { get; set; } = string.Empty;
    
    [Required]
    [MinLength(1, ErrorMessage = "At least one sale item is required")]
    public List<CreateSaleItemDto> SaleItems { get; set; } = new List<CreateSaleItemDto>();
    
    // Id is not required - backend will generate it automatically
}


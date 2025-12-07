using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClothsPosAPI.Models;

public class Item
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    public string CategoryId { get; set; } = string.Empty;
    
    [ForeignKey("CategoryId")]
    public Category? Category { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Cost { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Stock { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal MinStockLevel { get; set; } = 10;
    
    [StringLength(50)]
    public string? Barcode { get; set; }
    
    [Required]
    [StringLength(50)]
    public string SKU { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? Code { get; set; }
    
    [StringLength(100)]
    public string? Brand { get; set; }
    
    [StringLength(50)]
    public string ProductUnit { get; set; } = "piece";
    
    [StringLength(50)]
    public string SaleUnit { get; set; } = "piece";
    
    [StringLength(50)]
    public string PurchaseUnit { get; set; } = "piece";
    
    public string Images { get; set; } = "[]"; // JSON array of image URLs
    
    public string Sizes { get; set; } = "[]"; // JSON array of Size objects
    
    public string Colors { get; set; } = "[]"; // JSON array of Color objects
    
    public string? Note { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}



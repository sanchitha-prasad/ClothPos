using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ClothsPosAPI.Models;

public class SaleItem
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string SaleId { get; set; } = string.Empty;
    
    [ForeignKey("SaleId")]
    [JsonIgnore]
    public Sale? Sale { get; set; }
    
    [Required]
    public string ItemId { get; set; } = string.Empty;
    
    [ForeignKey("ItemId")]
    [JsonIgnore]
    public Item? Item { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Quantity { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }
}



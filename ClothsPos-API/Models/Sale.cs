using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ClothsPosAPI.Models;

public class Sale
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public DateTime Date { get; set; } = DateTime.UtcNow;
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Tax { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }
    
    [Required]
    [StringLength(50)]
    public string PaymentMethod { get; set; } = "cash";
    
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "completed"; // completed, pending, refunded, voided
    
    [StringLength(200)]
    public string? CustomerName { get; set; }
    
    [Required]
    public string CashierId { get; set; } = string.Empty;
    
    [ForeignKey("CashierId")]
    [JsonIgnore]
    public User? Cashier { get; set; }
    
    // Note: SaleItems is not marked with JsonIgnore because we need to access it in controllers
    // But we always return DTOs, not the entity directly, so circular references are avoided
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}



using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClothsPosAPI.Models;

public class PaymentDue
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string SaleId { get; set; } = string.Empty;
    
    [ForeignKey("SaleId")]
    public Sale? Sale { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    [Required]
    public DateTime DueDate { get; set; }
    
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "pending"; // pending, paid, overdue
}



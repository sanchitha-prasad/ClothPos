using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClothsPosAPI.Models;

public class ShopSettings
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [StringLength(200)]
    public string ShopName { get; set; } = "My Shop";
    
    public string? Logo { get; set; }
    
    [Required]
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string State { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string ZipCode { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Country { get; set; } = "Sri Lanka";
    
    [Required]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(5,2)")]
    public decimal TaxRate { get; set; } = 0;
    
    [StringLength(20)]
    public string RoundingRule { get; set; } = "none"; // none, round, ceil, floor
    
    public string ReceiptTemplate { get; set; } = "default";
    
    [StringLength(100)]
    public string? PrinterName { get; set; }
    
    // Currency Settings
    [StringLength(10)]
    public string CurrencySymbol { get; set; } = "Rs.";
    
    [StringLength(3)]
    public string CurrencyCode { get; set; } = "LKR";
    
    [StringLength(10)]
    public string CurrencyPosition { get; set; } = "before"; // before, after
    
    public string POSDevices { get; set; } = "[]"; // JSON array
    
    // Optional timestamp fields - only set if columns exist in database
    public DateTime? CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
}


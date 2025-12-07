using System.ComponentModel.DataAnnotations;

namespace ClothsPosAPI.DTOs;

public class ShopSettingsDto
{
    public string? Id { get; set; }
    
    [Required(ErrorMessage = "Shop name is required")]
    [StringLength(200)]
    public string ShopName { get; set; } = string.Empty;
    
    public string? Logo { get; set; }
    
    [Required(ErrorMessage = "Address is required")]
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "City is required")]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "State is required")]
    [StringLength(100)]
    public string State { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Zip code is required")]
    [StringLength(20)]
    public string ZipCode { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Country is required")]
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Phone is required")]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;
    
    [Range(0, 100, ErrorMessage = "Tax rate must be between 0 and 100")]
    public decimal TaxRate { get; set; } = 0;
    
    [StringLength(20)]
    public string RoundingRule { get; set; } = "none";
    
    public string ReceiptTemplate { get; set; } = "default";
    
    [StringLength(100)]
    public string? PrinterName { get; set; }
    
    [StringLength(10)]
    public string CurrencySymbol { get; set; } = "Rs.";
    
    [StringLength(3)]
    public string CurrencyCode { get; set; } = "LKR";
    
    [StringLength(10)]
    public string CurrencyPosition { get; set; } = "before";
    
    // POS Devices - array of device objects
    public List<POSDeviceDto>? PosDevices { get; set; }
}


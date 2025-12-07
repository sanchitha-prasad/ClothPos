using System.ComponentModel.DataAnnotations;

namespace ClothsPosAPI.Models;

public class Color
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(7)]
    public string HexCode { get; set; } = "#000000";
}



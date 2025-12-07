using System.ComponentModel.DataAnnotations;

namespace ClothsPosAPI.Models;

public class Size
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
}



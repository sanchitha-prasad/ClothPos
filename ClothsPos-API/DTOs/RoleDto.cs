using System.ComponentModel.DataAnnotations;

namespace ClothsPosAPI.DTOs;

public class RoleDto
{
    public string? Id { get; set; }
    
    [Required(ErrorMessage = "Role name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 50 characters")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? Description { get; set; }
    
    public string[] Permissions { get; set; } = Array.Empty<string>();
    
    public bool IsActive { get; set; } = true;
    
    public bool IsSystemRole { get; set; } = false;
    
    public int DisplayOrder { get; set; } = 0;
}

public class CreateRoleDto : RoleDto
{
}

public class UpdateRoleDto : RoleDto
{
    [Required]
    public new string Id { get; set; } = string.Empty;
}


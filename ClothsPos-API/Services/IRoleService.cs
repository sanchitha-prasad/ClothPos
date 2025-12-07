using ClothsPosAPI.DTOs;
using ClothsPosAPI.Models;

namespace ClothsPosAPI.Services;

public interface IRoleService
{
    Task<IEnumerable<Role>> GetAllRolesAsync();
    Task<Role?> GetRoleByIdAsync(string id);
    Task<Role> CreateRoleAsync(CreateRoleDto roleDto);
    Task<Role?> UpdateRoleAsync(string id, UpdateRoleDto roleDto);
    Task<bool> DeleteRoleAsync(string id);
}


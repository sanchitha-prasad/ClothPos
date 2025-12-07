using Microsoft.EntityFrameworkCore;
using ClothsPosAPI.Data;
using ClothsPosAPI.DTOs;
using ClothsPosAPI.Models;
using System.Text.Json;

namespace ClothsPosAPI.Services;

public class RoleService : IRoleService
{
    private readonly ApplicationDbContext _context;

    public RoleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Role>> GetAllRolesAsync()
    {
        return await _context.Roles
            .OrderBy(r => r.DisplayOrder)
            .ThenBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<Role?> GetRoleByIdAsync(string id)
    {
        return await _context.Roles.FindAsync(id);
    }

    public async Task<Role> CreateRoleAsync(CreateRoleDto roleDto)
    {
        // Check for duplicate name
        var existingRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name.ToLower() == roleDto.Name.ToLower().Trim());
        
        if (existingRole != null)
        {
            throw new InvalidOperationException("A role with this name already exists");
        }

        var role = new Role
        {
            Name = roleDto.Name.Trim(),
            Description = roleDto.Description?.Trim(),
            Permissions = JsonSerializer.Serialize(roleDto.Permissions),
            IsActive = roleDto.IsActive,
            IsSystemRole = roleDto.IsSystemRole,
            DisplayOrder = roleDto.DisplayOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        return role;
    }

    public async Task<Role?> UpdateRoleAsync(string id, UpdateRoleDto roleDto)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null) return null;

        // Check for duplicate name (excluding current role)
        var existingRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name.ToLower() == roleDto.Name.ToLower().Trim() && r.Id != id);
        
        if (existingRole != null)
        {
            throw new InvalidOperationException("A role with this name already exists");
        }

        // Prevent modifying system roles
        if (role.IsSystemRole && !roleDto.IsSystemRole)
        {
            throw new InvalidOperationException("Cannot remove system role flag from system roles");
        }

        role.Name = roleDto.Name.Trim();
        role.Description = roleDto.Description?.Trim();
        role.Permissions = JsonSerializer.Serialize(roleDto.Permissions);
        role.IsActive = roleDto.IsActive;
        role.DisplayOrder = roleDto.DisplayOrder;
        role.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<bool> DeleteRoleAsync(string id)
    {
        var role = await _context.Roles
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Id == id);
        
        if (role == null) return false;

        // Prevent deleting system roles
        if (role.IsSystemRole)
        {
            throw new InvalidOperationException("Cannot delete system roles");
        }

        // Check if role is assigned to any users
        if (role.Users.Any())
        {
            throw new InvalidOperationException("Cannot delete role that is assigned to users. Please reassign users first.");
        }

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
        return true;
    }
}


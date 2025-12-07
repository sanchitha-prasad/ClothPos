using Microsoft.EntityFrameworkCore;
using ClothsPosAPI.Data;
using ClothsPosAPI.DTOs;
using ClothsPosAPI.Models;
using System.Text.Json;

namespace ClothsPosAPI.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync(string? roleId = null, int page = 1, int limit = 100)
    {
        var query = _context.Users
            .Include(u => u.Role)
            .AsQueryable();

        if (!string.IsNullOrEmpty(roleId))
        {
            query = query.Where(u => u.RoleId == roleId);
        }

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(string id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User> CreateUserAsync(CreateUserDto userDto)
    {
        // Check for duplicate email
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("This email is already in use");
        }

        // Check for duplicate username
        var existingUsername = await _context.Users.FirstOrDefaultAsync(u => u.Username == userDto.Username);
        if (existingUsername != null)
        {
            throw new InvalidOperationException("This username is already in use");
        }

        // Verify role exists
        var role = await _context.Roles.FindAsync(userDto.RoleId);
        if (role == null)
        {
            throw new InvalidOperationException("Selected role does not exist");
        }

        var user = new User
        {
            Username = userDto.Username,
            Email = userDto.Email,
            RoleId = userDto.RoleId,
            Passcode = BCrypt.Net.BCrypt.HashPassword(userDto.Passcode),
            Permissions = JsonSerializer.Serialize(userDto.Permissions),
            IsActive = userDto.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<User?> UpdateUserAsync(string id, UpdateUserDto userDto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return null;

        // Check for duplicate email (excluding current user)
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email && u.Id != id);
        if (existingUser != null)
        {
            throw new InvalidOperationException("This email is already in use");
        }

        // Check for duplicate username (excluding current user)
        var existingUsername = await _context.Users.FirstOrDefaultAsync(u => u.Username == userDto.Username && u.Id != id);
        if (existingUsername != null)
        {
            throw new InvalidOperationException("This username is already in use");
        }

        // Verify role exists if changed
        if (user.RoleId != userDto.RoleId)
        {
            var role = await _context.Roles.FindAsync(userDto.RoleId);
            if (role == null)
            {
                throw new InvalidOperationException("Selected role does not exist");
            }
        }

        user.Username = userDto.Username;
        user.Email = userDto.Email;
        user.RoleId = userDto.RoleId;
        user.Permissions = JsonSerializer.Serialize(userDto.Permissions);
        user.IsActive = userDto.IsActive;

        // Update password if provided
        if (!string.IsNullOrEmpty(userDto.Passcode))
        {
            user.Passcode = BCrypt.Net.BCrypt.HashPassword(userDto.Passcode);
        }

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(string id, string oldPassword, string newPassword)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.Passcode))
        {
            throw new UnauthorizedAccessException("Current password is incorrect");
        }

        user.Passcode = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}



using ClothsPosAPI.DTOs;
using ClothsPosAPI.Models;

namespace ClothsPosAPI.Services;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllUsersAsync(string? role = null, int page = 1, int limit = 100);
    Task<User?> GetUserByIdAsync(string id);
    Task<User> CreateUserAsync(CreateUserDto userDto);
    Task<User?> UpdateUserAsync(string id, UpdateUserDto userDto);
    Task<bool> DeleteUserAsync(string id);
    Task<bool> ChangePasswordAsync(string id, string oldPassword, string newPassword);
}



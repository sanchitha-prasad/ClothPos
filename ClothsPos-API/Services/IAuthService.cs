using ClothsPosAPI.DTOs;

namespace ClothsPosAPI.Services;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginDto loginDto); // Web portal - Admin only
    Task<LoginResponseDto?> MobileLoginAsync(LoginDto loginDto); // Mobile app - Any active user
}



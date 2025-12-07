using ClothsPosAPI.Models;

namespace ClothsPosAPI.Services;

public interface ISettingsService
{
    Task<ShopSettings?> GetSettingsAsync();
    Task<ShopSettings> UpdateSettingsAsync(ShopSettings settings);
}



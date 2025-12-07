using ClothsPosAPI.DTOs;
using ClothsPosAPI.Models;

namespace ClothsPosAPI.Services;

public interface IItemService
{
    Task<IEnumerable<Item>> GetAllItemsAsync(string? search = null, string? categoryId = null, int page = 1, int limit = 100);
    Task<Item?> GetItemByIdAsync(string id);
    Task<Item> CreateItemAsync(CreateItemDto itemDto);
    Task<Item?> UpdateItemAsync(string id, UpdateItemDto itemDto);
    Task<bool> DeleteItemAsync(string id);
    Task<IEnumerable<Item>> GetLowStockItemsAsync();
    Task<IEnumerable<string>> UploadImagesAsync(string itemId, IFormFileCollection files);
}



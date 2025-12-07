using ClothsPosAPI.DTOs;
using ClothsPosAPI.Models;

namespace ClothsPosAPI.Services;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(string id);
    Task<Category> CreateCategoryAsync(CreateCategoryDto categoryDto);
    Task<Category?> UpdateCategoryAsync(string id, UpdateCategoryDto categoryDto);
    Task<bool> DeleteCategoryAsync(string id);
}



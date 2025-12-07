using Microsoft.EntityFrameworkCore;
using ClothsPosAPI.Data;
using ClothsPosAPI.DTOs;
using ClothsPosAPI.Models;

namespace ClothsPosAPI.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;

    public CategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(string id)
    {
        return await _context.Categories.FindAsync(id);
    }

    public async Task<Category> CreateCategoryAsync(CreateCategoryDto categoryDto)
    {
        // Check for duplicate name
        var existingCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.Name.ToLower() == categoryDto.Name.ToLower().Trim());
        
        if (existingCategory != null)
        {
            throw new InvalidOperationException("A category with this name already exists");
        }

        var category = new Category
        {
            Name = categoryDto.Name.Trim(),
            Description = categoryDto.Description?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return category;
    }

    public async Task<Category?> UpdateCategoryAsync(string id, UpdateCategoryDto categoryDto)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return null;

        // Check for duplicate name (excluding current category)
        var existingCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.Name.ToLower() == categoryDto.Name.ToLower().Trim() && c.Id != id);
        
        if (existingCategory != null)
        {
            throw new InvalidOperationException("A category with this name already exists");
        }

        category.Name = categoryDto.Name.Trim();
        category.Description = categoryDto.Description?.Trim();
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<bool> DeleteCategoryAsync(string id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return false;

        // Check if category is used by any items
        var hasItems = await _context.Items.AnyAsync(i => i.CategoryId == id);
        if (hasItems)
        {
            throw new InvalidOperationException("Cannot delete category that has associated items");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }
}



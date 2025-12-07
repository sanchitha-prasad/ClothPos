using Microsoft.EntityFrameworkCore;
using ClothsPosAPI.Data;
using ClothsPosAPI.DTOs;
using ClothsPosAPI.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace ClothsPosAPI.Services;

public class ItemService : IItemService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ItemService(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<IEnumerable<Item>> GetAllItemsAsync(string? search = null, string? categoryId = null, int page = 1, int limit = 100)
    {
        var query = _context.Items.Include(i => i.Category).AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(i => 
                i.Name.Contains(search) || 
                i.SKU.Contains(search) || 
                (i.Code != null && i.Code.Contains(search)));
        }

        if (!string.IsNullOrEmpty(categoryId))
        {
            query = query.Where(i => i.CategoryId == categoryId);
        }

        return await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<Item?> GetItemByIdAsync(string id)
    {
        return await _context.Items
            .Include(i => i.Category)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Item> CreateItemAsync(CreateItemDto itemDto)
    {
        // Check for duplicate SKU
        var existingItem = await _context.Items.FirstOrDefaultAsync(i => i.SKU == itemDto.SKU);
        if (existingItem != null)
        {
            throw new InvalidOperationException("A product with this SKU already exists");
        }

        var item = new Item
        {
            Name = itemDto.Name,
            CategoryId = itemDto.CategoryId,
            SKU = itemDto.SKU,
            Code = itemDto.SKU,
            Brand = itemDto.Brand,
            Price = itemDto.Price,
            Stock = itemDto.Stock,
            Cost = 0,
            MinStockLevel = 10,
            ProductUnit = "piece",
            SaleUnit = "piece",
            PurchaseUnit = "piece",
            Note = itemDto.Note,
            Barcode = itemDto.Barcode,
            Images = JsonSerializer.Serialize(itemDto.Images ?? Array.Empty<string>()),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        return item;
    }

    public async Task<Item?> UpdateItemAsync(string id, UpdateItemDto itemDto)
    {
        var item = await _context.Items.FindAsync(id);
        if (item == null) return null;

        // Check for duplicate SKU (excluding current item)
        var existingItem = await _context.Items.FirstOrDefaultAsync(i => i.SKU == itemDto.SKU && i.Id != id);
        if (existingItem != null)
        {
            throw new InvalidOperationException("A product with this SKU already exists");
        }

        item.Name = itemDto.Name;
        item.CategoryId = itemDto.CategoryId;
        item.SKU = itemDto.SKU;
        item.Code = itemDto.SKU;
        item.Brand = itemDto.Brand;
        item.Price = itemDto.Price;
        item.Stock = itemDto.Stock;
        item.Note = itemDto.Note;
        
        // Update barcode if provided
        if (itemDto.Barcode != null)
        {
            item.Barcode = itemDto.Barcode;
        }
        
        // Update images - merge with existing if new images are provided
        if (itemDto.Images != null && itemDto.Images.Length > 0)
        {
            // Replace images array with the new one (frontend sends complete list)
            item.Images = JsonSerializer.Serialize(itemDto.Images);
        }
        // If Images is null or empty, keep existing images (don't clear them)

        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<bool> DeleteItemAsync(string id)
    {
        var item = await _context.Items.FindAsync(id);
        if (item == null) return false;

        _context.Items.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Item>> GetLowStockItemsAsync()
    {
        return await _context.Items
            .Where(i => i.Stock <= i.MinStockLevel && i.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> UploadImagesAsync(string itemId, IFormFileCollection files)
    {
        var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "items", itemId);
        Directory.CreateDirectory(uploadsFolder);

        var imageUrls = new List<string>();

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var imageUrl = $"/uploads/items/{itemId}/{fileName}";
                imageUrls.Add(imageUrl);
            }
        }

        return imageUrls;
    }
}


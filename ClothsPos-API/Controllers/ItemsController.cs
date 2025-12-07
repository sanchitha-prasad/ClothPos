using Microsoft.AspNetCore.Mvc;
using ClothsPosAPI.DTOs;
using ClothsPosAPI.Services;
using System.Text.Json;
using ClothsPosAPI.Models;

namespace ClothsPosAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IItemService _itemService;
    private readonly ILogger<ItemsController> _logger;

    public ItemsController(IItemService itemService, ILogger<ItemsController> logger)
    {
        _itemService = itemService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetItems([FromQuery] string? search, [FromQuery] string? categoryId, [FromQuery] int page = 1, [FromQuery] int limit = 100)
    {
        try
        {
            var items = await _itemService.GetAllItemsAsync(search, categoryId, page, limit);
            var result = items.Select(i => MapToDto(i));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching items");
            return StatusCode(500, new { message = "Error fetching items", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetItem(string id)
    {
        try
        {
            var item = await _itemService.GetItemByIdAsync(id);
            if (item == null)
            {
                return NotFound(new { message = "Item not found" });
            }
            return Ok(MapToDto(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching item");
            return StatusCode(500, new { message = "Error fetching item", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateItem([FromBody] CreateItemDto itemDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var item = await _itemService.CreateItemAsync(itemDto);
            return CreatedAtAction(nameof(GetItem), new { id = item.Id }, MapToDto(item));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating item");
            return StatusCode(500, new { message = "Error creating item", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<object>> UpdateItem(string id, [FromBody] UpdateItemDto itemDto)
    {
        try
        {
            if (id != itemDto.Id)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var item = await _itemService.UpdateItemAsync(id, itemDto);
            if (item == null)
            {
                return NotFound(new { message = "Item not found" });
            }

            return Ok(MapToDto(item));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating item");
            return StatusCode(500, new { message = "Error updating item", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItem(string id)
    {
        try
        {
            var result = await _itemService.DeleteItemAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Item not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting item");
            return StatusCode(500, new { message = "Error deleting item", error = ex.Message });
        }
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<object>>> GetLowStockItems()
    {
        try
        {
            var items = await _itemService.GetLowStockItemsAsync();
            var result = items.Select(i => MapToDto(i));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching low stock items");
            return StatusCode(500, new { message = "Error fetching low stock items", error = ex.Message });
        }
    }

    [HttpPost("{itemId}/upload-images")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<object>> UploadImages(string itemId, [FromForm] IFormFileCollection? images)
    {
        try
        {
            if (images == null || images.Count == 0)
            {
                return BadRequest(new { message = "No images provided" });
            }

            var imageUrls = await _itemService.UploadImagesAsync(itemId, images);
            
            // Update item with new image URLs
            var item = await _itemService.GetItemByIdAsync(itemId);
            if (item != null)
            {
                var existingImages = JsonSerializer.Deserialize<string[]>(item.Images) ?? Array.Empty<string>();
                var allImages = existingImages.Concat(imageUrls).ToArray();
                
                var updateDto = new UpdateItemDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    CategoryId = item.CategoryId,
                    SKU = item.SKU,
                    Brand = item.Brand,
                    Price = item.Price,
                    Stock = item.Stock,
                    Note = item.Note,
                    Images = allImages
                };
                
                await _itemService.UpdateItemAsync(itemId, updateDto);
            }

            return Ok(new { imageUrls });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading images");
            return StatusCode(500, new { message = "Error uploading images", error = ex.Message });
        }
    }

    private object MapToDto(Models.Item item)
    {
        var images = JsonSerializer.Deserialize<string[]>(item.Images) ?? Array.Empty<string>();
        var sizes = JsonSerializer.Deserialize<Models.Size[]>(item.Sizes) ?? Array.Empty<Models.Size>();
        var colors = JsonSerializer.Deserialize<Models.Color[]>(item.Colors) ?? Array.Empty<Models.Color>();

        return new
        {
            id = item.Id,
            name = item.Name,
            description = item.Description,
            categoryId = item.CategoryId,
            category = item.Category != null ? new
            {
                id = item.Category.Id,
                name = item.Category.Name,
                description = item.Category.Description
            } : null,
            price = item.Price,
            cost = item.Cost,
            stock = item.Stock,
            minStockLevel = item.MinStockLevel,
            barcode = item.Barcode,
            sku = item.SKU,
            code = item.Code,
            brand = item.Brand,
            productUnit = item.ProductUnit,
            saleUnit = item.SaleUnit,
            purchaseUnit = item.PurchaseUnit,
            images = images,
            sizes = sizes,
            colors = colors,
            note = item.Note,
            isActive = item.IsActive,
            createdAt = item.CreatedAt.ToString("yyyy-MM-dd"),
            updatedAt = item.UpdatedAt.ToString("yyyy-MM-dd")
        };
    }
}


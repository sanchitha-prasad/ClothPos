using Microsoft.AspNetCore.Mvc;
using ClothsPosAPI.DTOs;
using ClothsPosAPI.Services;

namespace ClothsPosAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetCategories()
    {
        try
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var result = categories.Select(c => new
            {
                id = c.Id,
                name = c.Name,
                description = c.Description,
                createdAt = c.CreatedAt.ToString("yyyy-MM-dd"),
                updatedAt = c.UpdatedAt.ToString("yyyy-MM-dd")
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching categories");
            return StatusCode(500, new { message = "Error fetching categories", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetCategory(string id)
    {
        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound(new { message = "Category not found" });
            }

            return Ok(new
            {
                id = category.Id,
                name = category.Name,
                description = category.Description,
                createdAt = category.CreatedAt.ToString("yyyy-MM-dd"),
                updatedAt = category.UpdatedAt.ToString("yyyy-MM-dd")
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching category");
            return StatusCode(500, new { message = "Error fetching category", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateCategory([FromBody] CreateCategoryDto categoryDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var category = await _categoryService.CreateCategoryAsync(categoryDto);
            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, new
            {
                id = category.Id,
                name = category.Name,
                description = category.Description,
                createdAt = category.CreatedAt.ToString("yyyy-MM-dd"),
                updatedAt = category.UpdatedAt.ToString("yyyy-MM-dd")
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(500, new { message = "Error creating category", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<object>> UpdateCategory(string id, [FromBody] UpdateCategoryDto categoryDto)
    {
        try
        {
            if (id != categoryDto.Id)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var category = await _categoryService.UpdateCategoryAsync(id, categoryDto);
            if (category == null)
            {
                return NotFound(new { message = "Category not found" });
            }

            return Ok(new
            {
                id = category.Id,
                name = category.Name,
                description = category.Description,
                createdAt = category.CreatedAt.ToString("yyyy-MM-dd"),
                updatedAt = category.UpdatedAt.ToString("yyyy-MM-dd")
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category");
            return StatusCode(500, new { message = "Error updating category", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(string id)
    {
        try
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Category not found" });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category");
            return StatusCode(500, new { message = "Error deleting category", error = ex.Message });
        }
    }
}



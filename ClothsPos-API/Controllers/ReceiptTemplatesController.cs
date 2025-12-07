using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClothsPosAPI.Services;
using ClothsPosAPI.Models;
using ClothsPosAPI.DTOs;

namespace ClothsPosAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReceiptTemplatesController : ControllerBase
{
    private readonly IReceiptTemplateService _templateService;
    private readonly ILogger<ReceiptTemplatesController> _logger;

    public ReceiptTemplatesController(IReceiptTemplateService templateService, ILogger<ReceiptTemplatesController> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetTemplates()
    {
        try
        {
            var templates = await _templateService.GetAllTemplatesAsync();
            var result = templates.Select(t => new
            {
                id = t.Id,
                name = t.Name,
                description = t.Description,
                content = t.Content,
                structure = t.Structure,
                isDefault = t.IsDefault,
                isActive = t.IsActive,
                createdAt = t.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                updatedAt = t.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss")
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching receipt templates");
            return StatusCode(500, new { message = "Error fetching receipt templates", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetTemplate(string id)
    {
        try
        {
            var template = await _templateService.GetTemplateByIdAsync(id);
            if (template == null)
            {
                return NotFound(new { message = "Template not found" });
            }

            return Ok(new
            {
                id = template.Id,
                name = template.Name,
                description = template.Description,
                content = template.Content,
                structure = template.Structure,
                isDefault = template.IsDefault,
                isActive = template.IsActive,
                createdAt = template.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                updatedAt = template.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss")
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching receipt template");
            return StatusCode(500, new { message = "Error fetching receipt template", error = ex.Message });
        }
    }

    [HttpGet("default")]
    public async Task<ActionResult<object>> GetDefaultTemplate()
    {
        try
        {
            var template = await _templateService.GetDefaultTemplateAsync();
            if (template == null)
            {
                return NotFound(new { message = "No default template found" });
            }

            return Ok(new
            {
                id = template.Id,
                name = template.Name,
                description = template.Description,
                content = template.Content,
                structure = template.Structure,
                isDefault = template.IsDefault,
                isActive = template.IsActive
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching default template");
            return StatusCode(500, new { message = "Error fetching default template", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateTemplate([FromBody] ReceiptTemplateDto dto)
    {
        try
        {
            var template = new ReceiptTemplate
            {
                Name = dto.Name,
                Description = dto.Description,
                Content = dto.Content,
                Structure = dto.Structure,
                IsDefault = dto.IsDefault,
                IsActive = dto.IsActive
            };

            var created = await _templateService.CreateTemplateAsync(template);
            return CreatedAtAction(nameof(GetTemplate), new { id = created.Id }, new
            {
                id = created.Id,
                name = created.Name,
                description = created.Description,
                content = created.Content,
                structure = created.Structure,
                isDefault = created.IsDefault,
                isActive = created.IsActive
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating receipt template");
            return StatusCode(500, new { message = "Error creating receipt template", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<object>> UpdateTemplate(string id, [FromBody] ReceiptTemplateDto dto)
    {
        try
        {
            var template = new ReceiptTemplate
            {
                Name = dto.Name,
                Description = dto.Description,
                Content = dto.Content,
                Structure = dto.Structure,
                IsDefault = dto.IsDefault,
                IsActive = dto.IsActive
            };

            var updated = await _templateService.UpdateTemplateAsync(id, template);
            if (updated == null)
            {
                return NotFound(new { message = "Template not found" });
            }

            return Ok(new
            {
                id = updated.Id,
                name = updated.Name,
                description = updated.Description,
                content = updated.Content,
                structure = updated.Structure,
                isDefault = updated.IsDefault,
                isActive = updated.IsActive
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating receipt template");
            return StatusCode(500, new { message = "Error updating receipt template", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTemplate(string id)
    {
        try
        {
            var result = await _templateService.DeleteTemplateAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Template not found" });
            }

            return Ok(new { message = "Template deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting receipt template");
            return StatusCode(500, new { message = "Error deleting receipt template", error = ex.Message });
        }
    }

    [HttpPost("{id}/set-default")]
    public async Task<IActionResult> SetDefaultTemplate(string id)
    {
        try
        {
            var result = await _templateService.SetDefaultTemplateAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Template not found" });
            }

            return Ok(new { message = "Default template set successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default template");
            return StatusCode(500, new { message = "Error setting default template", error = ex.Message });
        }
    }
}


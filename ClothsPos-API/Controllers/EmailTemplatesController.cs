using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClothsPosAPI.Services;
using ClothsPosAPI.Models;
using ClothsPosAPI.DTOs;

namespace ClothsPosAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmailTemplatesController : ControllerBase
{
    private readonly IEmailTemplateService _templateService;
    private readonly ILogger<EmailTemplatesController> _logger;

    public EmailTemplatesController(IEmailTemplateService templateService, ILogger<EmailTemplatesController> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetTemplates([FromQuery] string? type)
    {
        try
        {
            var templates = await _templateService.GetAllTemplatesAsync(type);
            var result = templates.Select(t => new
            {
                id = t.Id,
                name = t.Name,
                description = t.Description,
                subject = t.Subject,
                body = t.Body,
                structure = t.Structure,
                type = t.Type,
                isActive = t.IsActive,
                createdAt = t.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                updatedAt = t.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss")
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching email templates");
            return StatusCode(500, new { message = "Error fetching email templates", error = ex.Message });
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
                subject = template.Subject,
                body = template.Body,
                structure = template.Structure,
                type = template.Type,
                isActive = template.IsActive,
                createdAt = template.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                updatedAt = template.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss")
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching email template");
            return StatusCode(500, new { message = "Error fetching email template", error = ex.Message });
        }
    }

    [HttpGet("type/{type}")]
    public async Task<ActionResult<object>> GetTemplateByType(string type)
    {
        try
        {
            var template = await _templateService.GetTemplateByTypeAsync(type);
            if (template == null)
            {
                return NotFound(new { message = $"No active template found for type: {type}" });
            }

            return Ok(new
            {
                id = template.Id,
                name = template.Name,
                description = template.Description,
                subject = template.Subject,
                body = template.Body,
                structure = template.Structure,
                type = template.Type,
                isActive = template.IsActive
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching email template by type");
            return StatusCode(500, new { message = "Error fetching email template by type", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateTemplate([FromBody] EmailTemplateDto dto)
    {
        try
        {
            var template = new EmailTemplate
            {
                Name = dto.Name,
                Description = dto.Description,
                Subject = dto.Subject,
                Body = dto.Body,
                Structure = dto.Structure,
                Type = dto.Type,
                IsActive = dto.IsActive
            };

            var created = await _templateService.CreateTemplateAsync(template);
            return CreatedAtAction(nameof(GetTemplate), new { id = created.Id }, new
            {
                id = created.Id,
                name = created.Name,
                description = created.Description,
                subject = created.Subject,
                body = created.Body,
                structure = created.Structure,
                type = created.Type,
                isActive = created.IsActive
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating email template");
            return StatusCode(500, new { message = "Error creating email template", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<object>> UpdateTemplate(string id, [FromBody] EmailTemplateDto dto)
    {
        try
        {
            var template = new EmailTemplate
            {
                Name = dto.Name,
                Description = dto.Description,
                Subject = dto.Subject,
                Body = dto.Body,
                Structure = dto.Structure,
                Type = dto.Type,
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
                subject = updated.Subject,
                body = updated.Body,
                structure = updated.Structure,
                type = updated.Type,
                isActive = updated.IsActive
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating email template");
            return StatusCode(500, new { message = "Error updating email template", error = ex.Message });
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting email template");
            return StatusCode(500, new { message = "Error deleting email template", error = ex.Message });
        }
    }
}


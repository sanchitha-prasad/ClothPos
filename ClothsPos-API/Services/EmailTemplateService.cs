using Microsoft.EntityFrameworkCore;
using ClothsPosAPI.Data;
using ClothsPosAPI.Models;

namespace ClothsPosAPI.Services;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly ApplicationDbContext _context;

    public EmailTemplateService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EmailTemplate>> GetAllTemplatesAsync(string? type = null)
    {
        var query = _context.EmailTemplates.AsQueryable();
        
        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(t => t.Type == type);
        }

        return await query
            .OrderBy(t => t.Type)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<EmailTemplate?> GetTemplateByIdAsync(string id)
    {
        return await _context.EmailTemplates.FindAsync(id);
    }

    public async Task<EmailTemplate?> GetTemplateByTypeAsync(string type)
    {
        return await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.Type == type && t.IsActive);
    }

    public async Task<EmailTemplate> CreateTemplateAsync(EmailTemplate template)
    {
        if (string.IsNullOrEmpty(template.Id))
        {
            template.Id = Guid.NewGuid().ToString();
        }

        template.CreatedAt = DateTime.UtcNow;
        template.UpdatedAt = DateTime.UtcNow;

        _context.EmailTemplates.Add(template);
        await _context.SaveChangesAsync();
        return template;
    }

    public async Task<EmailTemplate?> UpdateTemplateAsync(string id, EmailTemplate template)
    {
        var existing = await _context.EmailTemplates.FindAsync(id);
        if (existing == null) return null;

        existing.Name = template.Name;
        existing.Description = template.Description;
        existing.Subject = template.Subject;
        existing.Body = template.Body;
        existing.Structure = template.Structure;
        existing.Type = template.Type;
        existing.IsActive = template.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteTemplateAsync(string id)
    {
        var template = await _context.EmailTemplates.FindAsync(id);
        if (template == null) return false;

        _context.EmailTemplates.Remove(template);
        await _context.SaveChangesAsync();
        return true;
    }
}


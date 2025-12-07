using Microsoft.EntityFrameworkCore;
using ClothsPosAPI.Data;
using ClothsPosAPI.Models;

namespace ClothsPosAPI.Services;

public class ReceiptTemplateService : IReceiptTemplateService
{
    private readonly ApplicationDbContext _context;

    public ReceiptTemplateService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ReceiptTemplate>> GetAllTemplatesAsync()
    {
        return await _context.ReceiptTemplates
            .OrderByDescending(t => t.IsDefault)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<ReceiptTemplate?> GetTemplateByIdAsync(string id)
    {
        return await _context.ReceiptTemplates.FindAsync(id);
    }

    public async Task<ReceiptTemplate?> GetDefaultTemplateAsync()
    {
        return await _context.ReceiptTemplates
            .FirstOrDefaultAsync(t => t.IsDefault && t.IsActive);
    }

    public async Task<ReceiptTemplate> CreateTemplateAsync(ReceiptTemplate template)
    {
        if (string.IsNullOrEmpty(template.Id))
        {
            template.Id = Guid.NewGuid().ToString();
        }

        template.CreatedAt = DateTime.UtcNow;
        template.UpdatedAt = DateTime.UtcNow;

        // If this is set as default, unset other defaults
        if (template.IsDefault)
        {
            var existingDefaults = await _context.ReceiptTemplates
                .Where(t => t.IsDefault)
                .ToListAsync();
            
            foreach (var existing in existingDefaults)
            {
                existing.IsDefault = false;
            }
        }

        _context.ReceiptTemplates.Add(template);
        await _context.SaveChangesAsync();
        return template;
    }

    public async Task<ReceiptTemplate?> UpdateTemplateAsync(string id, ReceiptTemplate template)
    {
        var existing = await _context.ReceiptTemplates.FindAsync(id);
        if (existing == null) return null;

        existing.Name = template.Name;
        existing.Description = template.Description;
        existing.Content = template.Content;
        existing.Structure = template.Structure;
        existing.IsActive = template.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        // Handle default template change
        if (template.IsDefault && !existing.IsDefault)
        {
            var existingDefaults = await _context.ReceiptTemplates
                .Where(t => t.IsDefault && t.Id != id)
                .ToListAsync();
            
            foreach (var def in existingDefaults)
            {
                def.IsDefault = false;
            }
            
            existing.IsDefault = true;
        }
        else if (!template.IsDefault && existing.IsDefault)
        {
            existing.IsDefault = false;
        }

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteTemplateAsync(string id)
    {
        var template = await _context.ReceiptTemplates.FindAsync(id);
        if (template == null) return false;

        // Don't allow deleting default template
        if (template.IsDefault)
        {
            throw new InvalidOperationException("Cannot delete the default template. Set another template as default first.");
        }

        _context.ReceiptTemplates.Remove(template);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetDefaultTemplateAsync(string id)
    {
        var template = await _context.ReceiptTemplates.FindAsync(id);
        if (template == null) return false;

        // Unset all other defaults
        var existingDefaults = await _context.ReceiptTemplates
            .Where(t => t.IsDefault && t.Id != id)
            .ToListAsync();
        
        foreach (var existing in existingDefaults)
        {
            existing.IsDefault = false;
        }

        template.IsDefault = true;
        template.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}


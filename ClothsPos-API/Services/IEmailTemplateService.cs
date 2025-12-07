using ClothsPosAPI.Models;

namespace ClothsPosAPI.Services;

public interface IEmailTemplateService
{
    Task<IEnumerable<EmailTemplate>> GetAllTemplatesAsync(string? type = null);
    Task<EmailTemplate?> GetTemplateByIdAsync(string id);
    Task<EmailTemplate?> GetTemplateByTypeAsync(string type);
    Task<EmailTemplate> CreateTemplateAsync(EmailTemplate template);
    Task<EmailTemplate?> UpdateTemplateAsync(string id, EmailTemplate template);
    Task<bool> DeleteTemplateAsync(string id);
}


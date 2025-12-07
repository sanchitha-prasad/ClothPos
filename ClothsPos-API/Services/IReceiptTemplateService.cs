using ClothsPosAPI.Models;

namespace ClothsPosAPI.Services;

public interface IReceiptTemplateService
{
    Task<IEnumerable<ReceiptTemplate>> GetAllTemplatesAsync();
    Task<ReceiptTemplate?> GetTemplateByIdAsync(string id);
    Task<ReceiptTemplate?> GetDefaultTemplateAsync();
    Task<ReceiptTemplate> CreateTemplateAsync(ReceiptTemplate template);
    Task<ReceiptTemplate?> UpdateTemplateAsync(string id, ReceiptTemplate template);
    Task<bool> DeleteTemplateAsync(string id);
    Task<bool> SetDefaultTemplateAsync(string id);
}


using Microsoft.EntityFrameworkCore;
using ClothsPosAPI.Data;
using ClothsPosAPI.Models;

namespace ClothsPosAPI.Services;

public class SettingsService : ISettingsService
{
    private readonly ApplicationDbContext _context;

    public SettingsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ShopSettings?> GetSettingsAsync()
    {
        return await _context.ShopSettings.FirstOrDefaultAsync();
    }

    public async Task<ShopSettings> UpdateSettingsAsync(ShopSettings settings)
    {
        var existing = await _context.ShopSettings.FirstOrDefaultAsync();
        if (existing == null)
        {
            settings.Id = Guid.NewGuid().ToString();
            // Don't set CreatedAt/UpdatedAt - columns may not exist in database
            _context.ShopSettings.Add(settings);
        }
        else
        {
            existing.ShopName = settings.ShopName;
            existing.Logo = settings.Logo;
            existing.Address = settings.Address;
            existing.City = settings.City;
            existing.State = settings.State;
            existing.ZipCode = settings.ZipCode;
            existing.Country = settings.Country;
            existing.Phone = settings.Phone;
            existing.Email = settings.Email;
            existing.TaxRate = settings.TaxRate;
            existing.RoundingRule = settings.RoundingRule;
            existing.ReceiptTemplate = settings.ReceiptTemplate;
            existing.PrinterName = settings.PrinterName;
            existing.CurrencySymbol = settings.CurrencySymbol;
            existing.CurrencyCode = settings.CurrencyCode;
            existing.CurrencyPosition = settings.CurrencyPosition;
            existing.POSDevices = settings.POSDevices;
            // Don't set UpdatedAt - column may not exist in database
            settings = existing;
        }

        await _context.SaveChangesAsync();
        return settings;
    }
}



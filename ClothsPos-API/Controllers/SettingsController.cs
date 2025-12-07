using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClothsPosAPI.Services;
using ClothsPosAPI.Models;
using ClothsPosAPI.DTOs;

namespace ClothsPosAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(ISettingsService settingsService, ILogger<SettingsController> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetSettings()
    {
        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            if (settings == null)
            {
                return NotFound(new { message = "Settings not found" });
            }

            return Ok(MapToDto(settings));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching settings");
            return StatusCode(500, new { message = "Error fetching settings", error = ex.Message });
        }
    }

    [HttpPut]
    public async Task<ActionResult<object>> UpdateSettings([FromBody] System.Text.Json.JsonElement jsonElement)
    {
        try
        {
            // Manually deserialize to handle posDevices properly
            var settingsDto = System.Text.Json.JsonSerializer.Deserialize<ShopSettingsDto>(
                jsonElement.GetRawText(),
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (settingsDto == null)
            {
                return BadRequest(new { message = "Invalid settings data" });
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(settingsDto.ShopName))
            {
                return BadRequest(new { message = "Shop name is required" });
            }

            // Extract POS devices from JSON element
            List<POSDeviceDto>? posDevices = null;
            if (jsonElement.TryGetProperty("posDevices", out var posDevicesElement) && posDevicesElement.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                posDevices = System.Text.Json.JsonSerializer.Deserialize<List<POSDeviceDto>>(posDevicesElement.GetRawText(), 
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            var settings = new ShopSettings
            {
                Id = settingsDto.Id ?? Guid.NewGuid().ToString(),
                ShopName = settingsDto.ShopName,
                Logo = settingsDto.Logo,
                Address = settingsDto.Address,
                City = settingsDto.City,
                State = settingsDto.State,
                ZipCode = settingsDto.ZipCode,
                Country = settingsDto.Country,
                Phone = settingsDto.Phone,
                Email = settingsDto.Email,
                TaxRate = settingsDto.TaxRate,
                RoundingRule = settingsDto.RoundingRule,
                ReceiptTemplate = settingsDto.ReceiptTemplate,
                PrinterName = settingsDto.PrinterName,
                CurrencySymbol = settingsDto.CurrencySymbol,
                CurrencyCode = settingsDto.CurrencyCode,
                CurrencyPosition = settingsDto.CurrencyPosition,
                POSDevices = System.Text.Json.JsonSerializer.Serialize(posDevices ?? new List<POSDeviceDto>())
            };

            var updatedSettings = await _settingsService.UpdateSettingsAsync(settings);
            return Ok(MapToDto(updatedSettings));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating settings");
            return StatusCode(500, new { message = "Error updating settings", error = ex.Message });
        }
    }

    private object MapToDto(ShopSettings settings)
    {
        return new
        {
            id = settings.Id,
            shopName = settings.ShopName,
            logo = settings.Logo,
            address = settings.Address,
            city = settings.City,
            state = settings.State,
            zipCode = settings.ZipCode,
            country = settings.Country,
            phone = settings.Phone,
            email = settings.Email,
            taxRate = settings.TaxRate,
            roundingRule = settings.RoundingRule,
            receiptTemplate = settings.ReceiptTemplate,
            printerName = settings.PrinterName,
            currencySymbol = settings.CurrencySymbol,
            currencyCode = settings.CurrencyCode,
            currencyPosition = settings.CurrencyPosition,
            posDevices = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement[]>(settings.POSDevices ?? "[]") 
                ?.Select(e => {
                    try
                    {
                        return new
                        {
                            id = e.GetProperty("id").GetString(),
                            name = e.GetProperty("name").GetString(),
                            type = e.GetProperty("type").GetString(),
                            isActive = e.TryGetProperty("isActive", out var isActive) ? isActive.GetBoolean() : true,
                            lastConnected = e.TryGetProperty("lastConnected", out var lastConnected) ? lastConnected.GetString() : (string?)null
                        };
                    }
                    catch
                    {
                        return (object?)null;
                    }
                })
                .Where(e => e != null)
                .ToArray() ?? Array.Empty<object>()
        };
    }
}



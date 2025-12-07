namespace ClothsPosAPI.DTOs;

public class POSDeviceDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "desktop" or "mobile"
    public bool IsActive { get; set; } = true;
    public string? LastConnected { get; set; }
}


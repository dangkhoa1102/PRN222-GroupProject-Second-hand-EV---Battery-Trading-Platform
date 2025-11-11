namespace BLL.DTOs;

public class BatteryListingDetailDto
{
    public int BatteryId { get; set; }
    public int SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string? SellerEmail { get; set; }
    public string? SellerPhoneNumber { get; set; }
    public string BatteryType { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Capacity { get; set; } = string.Empty;
    public string? Voltage { get; set; }
    public string Condition { get; set; } = string.Empty;
    public int? HealthPercentage { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ModerationNote { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public DateTime? PublishedDate { get; set; }
}


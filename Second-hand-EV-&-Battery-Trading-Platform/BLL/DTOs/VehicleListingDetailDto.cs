namespace BLL.DTOs;

public class VehicleListingDetailDto
{
    public int VehicleId { get; set; }
    public int SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string? SellerEmail { get; set; }
    public string? SellerPhoneNumber { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int? Year { get; set; }
    public string? BatteryCapacity { get; set; }
    public decimal Price { get; set; }
    public string Condition { get; set; } = string.Empty;
    public int? Mileage { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ModerationNote { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public DateTime? PublishedDate { get; set; }
}


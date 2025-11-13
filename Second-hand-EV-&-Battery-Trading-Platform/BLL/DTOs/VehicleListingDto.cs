namespace BLL.DTOs;

public class VehicleListingDto
{
    public int VehicleId { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int? Year { get; set; }
    public decimal Price { get; set; }
    public string Condition { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int SellerId { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? SellerName { get; set; }
    public string? SellerEmail { get; set; }
    public string? SellerPhoneNumber { get; set; }
    public string? BatteryCapacity { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public DateTime? PublishedDate { get; set; }
    public string? ModerationNote { get; set; }
}


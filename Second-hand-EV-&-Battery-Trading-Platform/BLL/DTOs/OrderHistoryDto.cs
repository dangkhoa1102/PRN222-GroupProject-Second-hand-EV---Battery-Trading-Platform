namespace BLL.DTOs;

public class OrderHistoryDto
{
    public int OrderId { get; set; }
    public int SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string? SellerEmail { get; set; }
    public decimal TotalAmount { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? ProductType { get; set; } // "Vehicle" or "Battery"
    public string? ProductName { get; set; }
    public int? ProductId { get; set; }
    public bool HasReview { get; set; }
    public int? ReviewId { get; set; }
}


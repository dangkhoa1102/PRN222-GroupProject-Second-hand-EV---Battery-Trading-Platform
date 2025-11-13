namespace BLL.DTOs;

public class ReviewDto
{
    public int ReviewId { get; set; }
    public int OrderId { get; set; }
    public int ReviewerId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public string? ReviewerEmail { get; set; }
    public int ReviewedUserId { get; set; }
    public string ReviewedUserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? ProductType { get; set; } // "Vehicle" or "Battery"
    public string? ProductName { get; set; } // Vehicle: "Brand Model" or Battery: "Brand Type"
    public int? ProductId { get; set; }
}


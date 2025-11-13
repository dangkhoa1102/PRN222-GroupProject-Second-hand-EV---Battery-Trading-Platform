namespace BLL.DTOs;

public class OrderDetailDto
{
    // Thông tin đơn hàng
    public int OrderId { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime? CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    // Thông tin người mua
    public int BuyerId { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;
    public string BuyerPhone { get; set; } = string.Empty;

    // Thông tin sản phẩm (Vehicle hoặc Battery)
    public string OrderType { get; set; } = string.Empty; // "Vehicle" hoặc "Battery"
    public int ItemId { get; set; } // VehicleId hoặc BatteryId
    public string ItemName { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public string ItemImageUrl { get; set; } = string.Empty;

    // Status
    public string Status => CompletedDate.HasValue ? "Completed" : "Pending";

    // Check xem có thể confirm/reject không
    public bool CanConfirmOrReject => !CompletedDate.HasValue;
}

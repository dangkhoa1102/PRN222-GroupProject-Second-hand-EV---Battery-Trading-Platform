namespace BLL.DTOs;

public class OrderListDto
{
    public int OrderId { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime? CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    // Thông tin sản phẩm (từ VehicleOrder hoặc BatteryOrder)
    public string OrderType { get; set; } = string.Empty; // "Vehicle" hoặc "Battery"
    public int ItemId { get; set; } // VehicleId hoặc BatteryId
    public string ItemName { get; set; } = string.Empty; // Tên xe hoặc pin

    // Tính toán status
    public string Status => CompletedDate.HasValue ? "Completed" : "Pending";
}

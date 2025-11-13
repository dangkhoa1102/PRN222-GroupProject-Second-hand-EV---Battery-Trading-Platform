namespace BLL.DTOs;

public class OrderListDto
{
    public int OrderId { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;
    public string SellerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime? CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public string OrderStatus { get; set; } = "Pending";

    // Thông tin sản phẩm (từ VehicleOrder hoặc BatteryOrder)
    public string OrderType { get; set; } = string.Empty; // "Vehicle" hoặc "Battery"
    public int ItemId { get; set; } // VehicleId hoặc BatteryId
    public string ItemName { get; set; } = string.Empty; // Tên xe hoặc pin

    // Tính toán status display
    public string Status => OrderStatus switch
    {
        "Pending" => "Chờ xác nhận",
        "Confirmed" => "Đã xác nhận",
        "Delivered" => "Đã giao hàng",
        "Completed" => "Hoàn thành",
        "Cancelled" => "Đã hủy",
        _ => OrderStatus
    };
}

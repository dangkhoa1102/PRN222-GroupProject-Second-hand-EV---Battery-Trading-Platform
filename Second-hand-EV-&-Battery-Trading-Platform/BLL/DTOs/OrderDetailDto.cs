namespace BLL.DTOs;

public class OrderDetailDto
{
    // Thông tin đơn hàng
    public int OrderId { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime? CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public string OrderStatus { get; set; } = "Pending";
    public string? CancellationReason { get; set; }

    // Thông tin người mua
    public int BuyerId { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;
    public string BuyerPhone { get; set; } = string.Empty;

    // Thông tin người bán
    public int SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string SellerEmail { get; set; } = string.Empty;
    public string SellerPhone { get; set; } = string.Empty;

    // Thông tin sản phẩm (Vehicle hoặc Battery)
    public string OrderType { get; set; } = string.Empty; // "Vehicle" hoặc "Battery"
    public int ItemId { get; set; } // VehicleId hoặc BatteryId
    public string ItemName { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public string ItemImageUrl { get; set; } = string.Empty;

    // Status display
    public string Status => OrderStatus switch
    {
        "Pending" => "Chờ xác nhận",
        "Confirmed" => "Đã xác nhận",
        "Paid" => "Đã chuyển tiền",
        "Delivering" => "Đang giao hàng",
        "Delivered" => "Đã giao hàng",
        "Completed" => "Hoàn thành",
        "Cancelled" => "Đã hủy",
        _ => OrderStatus
    };

    // Check xem có thể confirm/reject không (cho seller)
    public bool CanConfirmOrReject => OrderStatus == "Pending";
    
    // Check xem có thể chuyển tiền không (cho buyer)
    public bool CanPay => OrderStatus == "Confirmed";
    
    // Check xem có thể giao hàng không (cho seller)
    public bool CanShip => OrderStatus == "Paid";
    
    // Check xem có thể hoàn thành giao hàng không (cho seller)
    public bool CanCompleteShipment => OrderStatus == "Delivering";
    
    // Check xem có thể confirm delivery không (cho buyer)
    public bool CanConfirmDelivery => OrderStatus == "Delivered";
    
    // Check xem có thể cancel không (cho buyer)
    public bool CanCancel => OrderStatus == "Pending" || OrderStatus == "Confirmed";
    
    // Check xem seller có thể hủy đơn hàng không
    public bool CanSellerCancel => OrderStatus == "Pending" || OrderStatus == "Confirmed" || OrderStatus == "Paid" || OrderStatus == "Delivering";
}

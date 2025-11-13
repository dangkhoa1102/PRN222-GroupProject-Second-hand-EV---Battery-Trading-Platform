using BLL.DTOs;
using DAL.Models;
using DAL.Repository;

namespace BLL.Services;

public class OrderService : IOrderService
{
    public async Task<List<OrderListDto>> GetSellerOrdersAsync(int sellerId, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var orderRepository = new OrderRepository(context);
        var orders = await orderRepository.GetSellerOrdersAsync(sellerId, cancellationToken);

        var orderDtos = orders.Select(o =>
        {
            string orderType = "";
            int itemId = 0;
            string itemName = "";

            // Xác định loại order: Vehicle hoặc Battery
            if (o.VehicleOrder != null)
            {
                orderType = "Vehicle";
                itemId = o.VehicleOrder.VehicleId;
                var vehicle = o.VehicleOrder.Vehicle;
                itemName = vehicle != null ? $"{vehicle.Brand} {vehicle.Model}" : "Xe điện";
            }
            else if (o.BatteryOrder != null)
            {
                orderType = "Battery";
                itemId = o.BatteryOrder.BatteryId;
                var battery = o.BatteryOrder.Battery;
                itemName = battery != null ? $"{battery.Brand} {battery.BatteryType}" : "Pin";
            }

            return new OrderListDto
            {
                OrderId = o.OrderId,
                OrderType = orderType,
                ItemId = itemId,
                ItemName = itemName,
                BuyerName = o.Buyer?.FullName ?? "Không rõ",
                BuyerEmail = o.Buyer?.Email ?? "",
                SellerName = o.Seller?.FullName ?? "Không rõ",
                SellerEmail = o.Seller?.Email ?? "",
                TotalAmount = o.TotalAmount,
                PaymentMethod = o.PaymentMethod ?? "Chưa chọn",
                CreatedDate = o.CreatedDate,
                CompletedDate = o.CompletedDate,
                DeliveryDate = o.DeliveryDate,
                OrderStatus = o.OrderStatus ?? "Pending"
            };
        }).ToList();

        return orderDtos;
    }

    public async Task<OrderDetailDto?> GetOrderDetailAsync(int orderId, int sellerId, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var orderRepository = new OrderRepository(context);
        var order = await orderRepository.GetOrderDetailAsync(orderId, sellerId, cancellationToken);

        if (order == null)
        {
            return null;
        }

        // Load thông tin sản phẩm (Vehicle hoặc Battery)
        string orderType = "";
        int itemId = 0;
        string itemName = "";
        string itemDescription = "";
        string itemImageUrl = "";

        if (order.VehicleOrder != null)
        {
            orderType = "Vehicle";
            itemId = order.VehicleOrder.VehicleId;
            var vehicle = order.VehicleOrder.Vehicle;
            if (vehicle != null)
            {
                itemName = $"{vehicle.Brand} {vehicle.Model}";
                itemDescription = $"Năm: {vehicle.Year}, Pin: {vehicle.BatteryCapacity}, Tình trạng: {vehicle.Condition}";
                itemImageUrl = vehicle.ImageUrl ?? "";
            }
        }
        else if (order.BatteryOrder != null)
        {
            orderType = "Battery";
            itemId = order.BatteryOrder.BatteryId;
            var battery = order.BatteryOrder.Battery;
            if (battery != null)
            {
                itemName = $"{battery.Brand} {battery.BatteryType}";
                itemDescription = $"Dung lượng: {battery.Capacity}, Điện áp: {battery.Voltage}, Tình trạng: {battery.Condition}";
                itemImageUrl = battery.ImageUrl ?? "";
            }
        }

        return new OrderDetailDto
        {
            OrderId = order.OrderId,
            OrderType = orderType,
            ItemId = itemId,
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.PaymentMethod ?? "Chưa chọn",
            CreatedDate = order.CreatedDate,
            CompletedDate = order.CompletedDate,
            DeliveryDate = order.DeliveryDate,
            OrderStatus = order.OrderStatus ?? "Pending",
            CancellationReason = order.CancellationReason,
            BuyerId = order.BuyerId,
            BuyerName = order.Buyer?.FullName ?? "Không rõ",
            BuyerEmail = order.Buyer?.Email ?? "",
            BuyerPhone = order.Buyer?.PhoneNumber ?? "Không có",
            SellerId = order.SellerId,
            SellerName = order.Seller?.FullName ?? "Không rõ",
            SellerEmail = order.Seller?.Email ?? "",
            SellerPhone = order.Seller?.PhoneNumber ?? "Không có",
            ItemName = itemName,
            ItemDescription = itemDescription,
            ItemImageUrl = itemImageUrl
        };
    }

    public async Task<bool> ConfirmOrderAsync(int orderId, int sellerId, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var orderRepository = new OrderRepository(context);
        var order = await orderRepository.GetOrderDetailAsync(orderId, sellerId, cancellationToken);

        if (order == null)
        {
            return false;
        }

        if (order.OrderStatus != "Pending")
        {
            return false; // Đơn hàng đã được xử lý rồi
        }

        // Xác nhận đơn hàng (seller confirm)
        order.OrderStatus = "Confirmed";
        order.CompletedDate = DateTime.Now; // Seller confirmation date
        await orderRepository.UpdateOrderAsync(order);
        await orderRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> RejectOrderAsync(int orderId, int sellerId, string? reason, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var orderRepository = new OrderRepository(context);
        var order = await orderRepository.GetOrderDetailAsync(orderId, sellerId, cancellationToken);

        if (order == null)
        {
            return false;
        }

        if (order.OrderStatus != "Pending")
        {
            return false; // Đơn hàng đã được xử lý rồi
        }

        // Từ chối đơn hàng (không xóa, chỉ đánh dấu là Cancelled)
        order.OrderStatus = "Cancelled";
        order.CancellationReason = reason ?? "Người bán từ chối đơn hàng";
        await orderRepository.UpdateOrderAsync(order);
        await orderRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}


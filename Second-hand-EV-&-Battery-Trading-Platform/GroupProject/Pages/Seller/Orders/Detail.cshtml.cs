using BLL.DTOs;
using DAL.Models;
using DAL.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GroupProject.Pages.Seller.Orders;

public class DetailModel : PageModel
{
    private readonly EVTradingPlatformContext _context;

    public DetailModel(EVTradingPlatformContext context)
    {
        _context = context;
    }

    public OrderDetailDto OrderDetail { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var sellerId = HttpContext.Session.GetInt32("UserId");
        if (!sellerId.HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        var orderRepository = new OrderRepository(_context);
        var order = await orderRepository.GetOrderDetailAsync(id, sellerId.Value);

        if (order == null)
        {
            return NotFound();
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

        OrderDetail = new OrderDetailDto
        {
            OrderId = order.OrderId,
            OrderType = orderType,
            ItemId = itemId,
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.PaymentMethod ?? "Chưa chọn",
            CreatedDate = order.CreatedDate,
            CompletedDate = order.CompletedDate,
            BuyerId = order.BuyerId,
            BuyerName = order.Buyer?.FullName ?? "Không rõ",
            BuyerEmail = order.Buyer?.Email ?? "",
            BuyerPhone = order.Buyer?.PhoneNumber ?? "Không có",
            ItemName = itemName,
            ItemDescription = itemDescription,
            ItemImageUrl = itemImageUrl
        };

        return Page();
    }

    // Handler cho nút Confirm
    public async Task<IActionResult> OnPostConfirmAsync(int id)
    {
        var sellerId = HttpContext.Session.GetInt32("UserId");
        if (!sellerId.HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        var orderRepository = new OrderRepository(_context);
        var order = await orderRepository.GetOrderDetailAsync(id, sellerId.Value);

        if (order == null)
        {
            return NotFound();
        }

        // Kiểm tra đơn hàng đã hoàn thành chưa
        if (order.CompletedDate.HasValue)
        {
            TempData["Error"] = "Đơn hàng này đã được xử lý rồi!";
            return RedirectToPage(new { id = id });
        }

        // Xác nhận đơn hàng: set CompletedDate
        order.CompletedDate = DateTime.Now;

        await orderRepository.UpdateOrderAsync(order);
        await orderRepository.SaveChangesAsync();

        StatusMessage = "Đơn hàng đã được xác nhận thành công!";
        return RedirectToPage(new { id = id });
    }

    // Handler cho nút Reject
    public async Task<IActionResult> OnPostRejectAsync(int id, string? reason)
    {
        var sellerId = HttpContext.Session.GetInt32("UserId");
        if (!sellerId.HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        var orderRepository = new OrderRepository(_context);
        var order = await orderRepository.GetOrderDetailAsync(id, sellerId.Value);

        if (order == null)
        {
            return NotFound();
        }

        if (order.CompletedDate.HasValue)
        {
            TempData["Error"] = "Đơn hàng này đã được xử lý rồi!";
            return RedirectToPage(new { id = id });
        }

        // Xóa đơn hàng hoặc đánh dấu là rejected
        // Tùy vào thiết kế của bạn
        // Cách 1: Xóa đơn
        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        // Cách 2: Nếu có cột RejectionReason, lưu lại
        // order.RejectionReason = reason;
        // await orderRepository.SaveChangesAsync();

        TempData["Success"] = $"Đơn hàng #{id} đã bị từ chối. Lý do: {reason}";
        return RedirectToPage("/Seller/Orders/Index");
    }
}

using BLL.DTOs;
using DAL.Models;
using DAL.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GroupProject.Pages.Seller.Orders;

public class IndexModel : PageModel
{
    private readonly EVTradingPlatformContext _context;

    public IndexModel(EVTradingPlatformContext context)
    {
        _context = context;
    }

    public List<OrderListDto> PendingOrders { get; set; } = new();
    public List<OrderListDto> CompletedOrders { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // Kiểm tra đã login chưa
        var sellerId = HttpContext.Session.GetInt32("UserId");
        if (!sellerId.HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        // Kiểm tra role có phải Customer/Seller không
        var role = HttpContext.Session.GetString("Role");
        if (role != "Customer")
        {
            TempData["Error"] = "Bạn không có quyền truy cập trang này.";
            return RedirectToPage("/Index");
        }

        var orderRepository = new OrderRepository(_context);
        var orders = await orderRepository.GetSellerOrdersAsync(sellerId.Value);

        // Chuyển đổi sang DTO và phân loại
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
                TotalAmount = o.TotalAmount,
                PaymentMethod = o.PaymentMethod ?? "Chưa chọn",
                CreatedDate = o.CreatedDate,
                CompletedDate = o.CompletedDate
            };
        }).ToList();

        // Phân loại đơn hàng
        PendingOrders = orderDtos.Where(o => !o.CompletedDate.HasValue).ToList();
        CompletedOrders = orderDtos.Where(o => o.CompletedDate.HasValue).ToList();

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
            TempData["Error"] = "Không tìm thấy đơn hàng.";
            return RedirectToPage();
        }

        if (order.CompletedDate.HasValue)
        {
            TempData["Error"] = "Đơn hàng này đã được xử lý rồi!";
            return RedirectToPage();
        }

        // Xác nhận đơn hàng
        order.CompletedDate = DateTime.Now;
        await orderRepository.UpdateOrderAsync(order);
        await orderRepository.SaveChangesAsync();

        StatusMessage = $"Đơn hàng #{id} đã được xác nhận thành công!";
        return RedirectToPage();
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
            TempData["Error"] = "Không tìm thấy đơn hàng.";
            return RedirectToPage();
        }

        if (order.CompletedDate.HasValue)
        {
            TempData["Error"] = "Đơn hàng này đã được xử lý rồi!";
            return RedirectToPage();
        }

        // Xóa đơn hàng
        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        StatusMessage = $"Đơn hàng #{id} đã bị từ chối. Lý do: {reason ?? "Không rõ"}";
        return RedirectToPage();
    }
}

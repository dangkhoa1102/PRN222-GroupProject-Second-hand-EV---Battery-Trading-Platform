using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GroupProject.Pages.Seller.Orders;

public class IndexModel : PageModel
{
    private readonly IOrderService _orderService;

    public IndexModel(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public List<OrderListDto> PendingOrders { get; set; } = new();
    public List<OrderListDto> ConfirmedOrders { get; set; } = new();
    public List<OrderListDto> CompletedOrders { get; set; } = new();
    public List<OrderListDto> CancelledOrders { get; set; } = new();

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

        var orders = await _orderService.GetSellerOrdersAsync(sellerId.Value);

        // Phân loại đơn hàng theo OrderStatus
        PendingOrders = orders.Where(o => o.OrderStatus == "Pending").ToList();
        ConfirmedOrders = orders.Where(o => o.OrderStatus == "Confirmed").ToList();
        CompletedOrders = orders.Where(o => o.OrderStatus == "Completed").ToList();
        CancelledOrders = orders.Where(o => o.OrderStatus == "Cancelled").ToList();

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

        var result = await _orderService.ConfirmOrderAsync(id, sellerId.Value);

        if (!result)
        {
            TempData["Error"] = "Không thể xác nhận đơn hàng. Vui lòng kiểm tra lại.";
            return RedirectToPage();
        }

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

        var result = await _orderService.RejectOrderAsync(id, sellerId.Value, reason);

        if (!result)
        {
            TempData["Error"] = "Không thể từ chối đơn hàng. Vui lòng kiểm tra lại.";
            return RedirectToPage();
        }

        StatusMessage = $"Đơn hàng #{id} đã bị từ chối. Lý do: {reason ?? "Không rõ"}";
        return RedirectToPage();
    }
}

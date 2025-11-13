using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GroupProject.Pages.Seller.Orders;

public class DetailModel : PageModel
{
    private readonly IOrderService _orderService;

    public DetailModel(IOrderService orderService)
    {
        _orderService = orderService;
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

        var orderDetail = await _orderService.GetOrderDetailAsync(id, sellerId.Value);

        if (orderDetail == null)
        {
            return NotFound();
        }

        OrderDetail = orderDetail;

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
            return RedirectToPage(new { id = id });
        }

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

        var result = await _orderService.RejectOrderAsync(id, sellerId.Value, reason);

        if (!result)
        {
            TempData["Error"] = "Không thể từ chối đơn hàng. Vui lòng kiểm tra lại.";
            return RedirectToPage(new { id = id });
        }

        TempData["Success"] = $"Đơn hàng #{id} đã bị từ chối. Lý do: {reason}";
        return RedirectToPage("/Seller/Orders/Index");
    }
}

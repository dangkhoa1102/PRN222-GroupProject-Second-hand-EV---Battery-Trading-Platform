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

    // Handler cho nút Ship
    public async Task<IActionResult> OnPostShipAsync(int id)
    {
        var sellerId = HttpContext.Session.GetInt32("UserId");
        if (!sellerId.HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        var result = await _orderService.ShipOrderAsync(id, sellerId.Value);

        if (!result)
        {
            TempData["Error"] = "Không thể giao hàng. Vui lòng kiểm tra lại trạng thái đơn hàng.";
            return RedirectToPage(new { id = id });
        }

        StatusMessage = "Đơn hàng đã được chuyển sang trạng thái đang giao hàng!";
        return RedirectToPage(new { id = id });
    }

    // Handler cho nút Complete Shipment
    public async Task<IActionResult> OnPostCompleteShipmentAsync(int id)
    {
        var sellerId = HttpContext.Session.GetInt32("UserId");
        if (!sellerId.HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        var result = await _orderService.CompleteShipmentAsync(id, sellerId.Value);

        if (!result)
        {
            TempData["Error"] = "Không thể hoàn thành giao hàng. Vui lòng kiểm tra lại trạng thái đơn hàng.";
            return RedirectToPage(new { id = id });
        }

        StatusMessage = "Đơn hàng đã được đánh dấu là đã giao hàng! Người mua có 5 phút để xác nhận nhận hàng.";
        return RedirectToPage(new { id = id });
    }

    // Handler cho nút Reject
    public async Task<IActionResult> OnPostRejectAsync(int id, string? reason)
    {
        try
        {
            var sellerId = HttpContext.Session.GetInt32("UserId");
            if (!sellerId.HasValue)
            {
                TempData["Error"] = "Bạn cần đăng nhập để thực hiện thao tác này.";
                return RedirectToPage("/Account/Login");
            }

            // Validate reason
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "Vui lòng nhập lý do từ chối đơn hàng.";
                return RedirectToPage(new { id = id });
            }

            var result = await _orderService.RejectOrderAsync(id, sellerId.Value, reason);

            if (!result)
            {
                TempData["Error"] = "Không thể từ chối đơn hàng. Vui lòng kiểm tra lại trạng thái đơn hàng.";
                return RedirectToPage(new { id = id });
            }

            TempData["Success"] = $"Đơn hàng #{id} đã bị từ chối thành công.";
            return RedirectToPage("/Seller/Orders/Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Đã xảy ra lỗi khi từ chối đơn hàng: {ex.Message}";
            return RedirectToPage(new { id = id });
        }
    }

    // Handler cho nút Cancel
    public async Task<IActionResult> OnPostCancelAsync(int id, string? reason)
    {
        try
        {
            var sellerId = HttpContext.Session.GetInt32("UserId");
            if (!sellerId.HasValue)
            {
                TempData["Error"] = "Bạn cần đăng nhập để thực hiện thao tác này.";
                return RedirectToPage("/Account/Login");
            }

            // Validate reason
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "Vui lòng nhập lý do hủy đơn hàng.";
                return RedirectToPage(new { id = id });
            }

            var result = await _orderService.CancelOrderAsync(id, sellerId.Value, reason);

            if (!result)
            {
                TempData["Error"] = "Không thể hủy đơn hàng. Vui lòng kiểm tra lại trạng thái đơn hàng.";
                return RedirectToPage(new { id = id });
            }

            TempData["Success"] = $"Đơn hàng #{id} đã được hủy thành công.";
            return RedirectToPage("/Seller/Orders/Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Đã xảy ra lỗi khi hủy đơn hàng: {ex.Message}";
            return RedirectToPage(new { id = id });
        }
    }
}

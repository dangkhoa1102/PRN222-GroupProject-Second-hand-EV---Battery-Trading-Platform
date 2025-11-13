using BLL.Constants;
using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GroupProject.Pages.Buyer.Orders;

public class DetailModel : PageModel
{
    private readonly IBuyerOrderService _buyerOrderService;

    public DetailModel(IBuyerOrderService buyerOrderService)
    {
        _buyerOrderService = buyerOrderService;
    }

    public OrderDetailDto? OrderDetail { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? StatusMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id, string? message)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("Role");

        // Kiểm tra login
        if (!userId.HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        // Kiểm tra role
        if (role != RoleConstants.Customer)
        {
            ErrorMessage = "Chỉ khách hàng mới có thể xem đơn hàng.";
            return Page();
        }

        if (!string.IsNullOrEmpty(message))
        {
            StatusMessage = message;
        }

        try
        {
            OrderDetail = await _buyerOrderService.GetBuyerOrderDetailAsync(id, userId.Value);
            if (OrderDetail == null)
            {
                ErrorMessage = "Không tìm thấy đơn hàng hoặc bạn không có quyền xem đơn hàng này.";
                return Page();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi khi tải đơn hàng: {ex.Message}";
        }

        return Page();
    }

    // Handler cho nút Confirm Delivery
    public async Task<IActionResult> OnPostConfirmDeliveryAsync(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("Role");

        // Kiểm tra login
        if (!userId.HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        // Kiểm tra role
        if (role != RoleConstants.Customer)
        {
            ErrorMessage = "Chỉ khách hàng mới có thể xác nhận nhận hàng.";
            return RedirectToPage(new { id = id });
        }

        try
        {
            var result = await _buyerOrderService.ConfirmDeliveryAsync(id, userId.Value);
            if (result)
            {
                StatusMessage = "Đã xác nhận nhận hàng thành công! Đơn hàng đã hoàn thành.";
                return RedirectToPage(new { id = id, message = StatusMessage });
            }
            else
            {
                ErrorMessage = "Không thể xác nhận nhận hàng. Đơn hàng chưa được người bán xác nhận hoặc đã được xử lý.";
                return RedirectToPage(new { id = id });
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi: {ex.Message}";
            return RedirectToPage(new { id = id });
        }
    }

    // Handler cho nút Cancel Order
    public async Task<IActionResult> OnPostCancelOrderAsync(int id, string? reason)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("Role");

        // Kiểm tra login
        if (!userId.HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        // Kiểm tra role
        if (role != RoleConstants.Customer)
        {
            ErrorMessage = "Chỉ khách hàng mới có thể hủy đơn hàng.";
            return RedirectToPage(new { id = id });
        }

        try
        {
            var result = await _buyerOrderService.CancelOrderAsync(id, userId.Value, reason);
            if (result)
            {
                StatusMessage = "Đã hủy đơn hàng thành công!";
                return RedirectToPage(new { id = id, message = StatusMessage });
            }
            else
            {
                ErrorMessage = "Không thể hủy đơn hàng. Đơn hàng đã hoàn thành hoặc đã bị hủy trước đó.";
                return RedirectToPage(new { id = id });
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi: {ex.Message}";
            return RedirectToPage(new { id = id });
        }
    }
}


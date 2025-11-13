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

    public List<OrderListDto> AllOrders { get; set; } = new();

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

        // Hiển thị tất cả đơn hàng, sắp xếp theo ngày tạo mới nhất
        AllOrders = orders.OrderByDescending(o => o.CreatedDate).ToList();

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
                return RedirectToPage();
            }

            var result = await _orderService.RejectOrderAsync(id, sellerId.Value, reason);

            if (!result)
            {
                TempData["Error"] = "Không thể từ chối đơn hàng. Vui lòng kiểm tra lại trạng thái đơn hàng.";
                return RedirectToPage();
            }

            TempData["Success"] = $"Đơn hàng #{id} đã bị từ chối thành công.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Đã xảy ra lỗi khi từ chối đơn hàng: {ex.Message}";
            return RedirectToPage();
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
                return RedirectToPage();
            }

            var result = await _orderService.CancelOrderAsync(id, sellerId.Value, reason);

            if (!result)
            {
                TempData["Error"] = "Không thể hủy đơn hàng. Vui lòng kiểm tra lại trạng thái đơn hàng.";
                return RedirectToPage();
            }

            TempData["Success"] = $"Đơn hàng #{id} đã được hủy thành công.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Đã xảy ra lỗi khi hủy đơn hàng: {ex.Message}";
            return RedirectToPage();
        }
    }
}

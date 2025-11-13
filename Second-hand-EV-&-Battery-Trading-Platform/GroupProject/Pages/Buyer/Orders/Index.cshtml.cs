using BLL.Constants;
using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GroupProject.Pages.Buyer.Orders;

public class IndexModel : PageModel
{
    private readonly IBuyerOrderService _buyerOrderService;

    public IndexModel(IBuyerOrderService buyerOrderService)
    {
        _buyerOrderService = buyerOrderService;
    }

    public List<OrderListDto> Orders { get; private set; } = new();
    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(string? message)
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
            Orders = await _buyerOrderService.GetBuyerOrdersAsync(userId.Value);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi khi tải danh sách đơn hàng: {ex.Message}";
        }

        return Page();
    }
}


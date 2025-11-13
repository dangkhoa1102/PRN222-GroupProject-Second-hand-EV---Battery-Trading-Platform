using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GroupProject.Pages.Reviews;

public class OrderHistoryModel : PageModel
{
    private readonly IReviewService _reviewService;

    public OrderHistoryModel(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    public List<OrderHistoryDto> Orders { get; private set; } = new();
    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        Orders = await _reviewService.GetOrderHistoryAsync(userId.Value);
        return Page();
    }
}


using BLL.DTOs;
using BLL.Services;
using GroupProject.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GroupProject.Pages.Reviews;

public class CreateModel : PageModel
{
    private readonly IReviewService _reviewService;
    private readonly INotificationService _notificationService;

    public CreateModel(IReviewService reviewService, INotificationService notificationService)
    {
        _reviewService = reviewService;
        _notificationService = notificationService;
    }

    [BindProperty]
    public CreateReviewDto Input { get; set; } = new();

    public int OrderId { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(int orderId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        OrderId = orderId;
        Input.OrderId = orderId;

        // Kiểm tra xem có thể review không
        var canReview = await _reviewService.CanReviewOrderAsync(orderId, userId.Value);
        if (!canReview)
        {
            ErrorMessage = "Bạn không thể đánh giá đơn hàng này. Đơn hàng phải đã hoàn thành và bạn chưa đánh giá.";
            return Page();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        if (!ModelState.IsValid)
        {
            OrderId = Input.OrderId;
            return Page();
        }

        var result = await _reviewService.CreateReviewAsync(userId.Value, Input);

        if (!result.IsSuccess)
        {
            ErrorMessage = result.ErrorMessage;
            OrderId = Input.OrderId;
            return Page();
        }

        // Lấy thông tin review để gửi notification
        if (result.IsSuccess && result.ReviewId.HasValue)
        {
            var review = await _reviewService.GetReviewByIdAsync(result.ReviewId.Value);
            if (review != null)
            {
                var reviewerName = HttpContext.Session.GetString("FullName") ?? "Người dùng";
                await _notificationService.NotifyNewReviewAsync(result.ReviewId.Value, review.ReviewedUserId, reviewerName, review.Rating);
            }
        }

        TempData["ReviewSuccess"] = "Đánh giá của bạn đã được gửi thành công!";
        return RedirectToPage("OrderHistory");
    }
}


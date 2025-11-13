using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GroupProject.Pages.Reviews;

public class DetailsModel : PageModel
{
    private readonly IReviewService _reviewService;

    public DetailsModel(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    public ReviewDto? Review { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Review = await _reviewService.GetReviewByIdAsync(id);

        if (Review == null)
        {
            ErrorMessage = "Đánh giá không tồn tại.";
            return Page();
        }

        return Page();
    }

    public string GetRatingStars(int rating)
    {
        return new string('★', rating) + new string('☆', 5 - rating);
    }
}


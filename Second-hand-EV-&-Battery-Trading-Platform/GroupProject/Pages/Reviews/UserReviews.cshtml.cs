using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GroupProject.Pages.Reviews;

public class UserReviewsModel : PageModel
{
    private readonly IReviewService _reviewService;

    public UserReviewsModel(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    public List<ReviewDto> Reviews { get; private set; } = new();
    public UserReviewSummaryDto? Summary { get; private set; }
    public int? RatingFilter { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(int userId, int? rating = null)
    {
        RatingFilter = rating;

        var reviews = await _reviewService.GetReviewsByUserIdAsync(userId, rating);
        Reviews = reviews;

        Summary = await _reviewService.GetUserReviewSummaryAsync(userId);

        return Page();
    }

    public string GetRatingStars(int rating)
    {
        return new string('★', rating) + new string('☆', 5 - rating);
    }
}


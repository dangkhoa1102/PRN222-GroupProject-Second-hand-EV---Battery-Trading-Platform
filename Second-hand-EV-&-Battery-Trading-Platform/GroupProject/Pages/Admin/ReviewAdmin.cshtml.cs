using BLL.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.DTOs;

namespace GroupProject.Pages.Admin
{
    public class ReviewAdminModel : PageModel
    {
        private readonly IAdminReviewService _reviewService;

        public ReviewAdminModel(IAdminReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        public List<AdminReviewDto> Reviews { get; private set; } = new();

        public async Task OnGetAsync()
        {
            Reviews = await _reviewService.GetAllReviewsAsync();
        }
    }
}

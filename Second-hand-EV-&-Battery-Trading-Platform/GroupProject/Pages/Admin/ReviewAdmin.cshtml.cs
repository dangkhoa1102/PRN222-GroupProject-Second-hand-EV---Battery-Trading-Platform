using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _reviewService.DeleteReviewAsync(id);

            return RedirectToPage(); // load l?i list
        }
    }
}

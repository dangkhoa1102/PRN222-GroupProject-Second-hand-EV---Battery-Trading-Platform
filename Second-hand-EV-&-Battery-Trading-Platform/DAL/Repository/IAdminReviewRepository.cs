using DAL.Models;

namespace DAL.Repositories.Admin
{
    public interface IAdminReviewRepository
    {
        Task<List<Review>> GetAllReviewsAsync();
        Task DeleteReviewAsync(int reviewId);
    }
}

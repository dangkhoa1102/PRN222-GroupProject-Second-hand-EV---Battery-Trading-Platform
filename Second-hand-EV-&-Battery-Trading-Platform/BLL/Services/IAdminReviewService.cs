using BLL.DTOs;

namespace BLL.Services
{
    public interface IAdminReviewService
    {
        Task<List<AdminReviewDto>> GetAllReviewsAsync();
    }
}

using BLL.DTOs;

namespace BLL.Services;

public interface IReviewService
{
    Task<ReviewResultDto> CreateReviewAsync(int reviewerId, CreateReviewDto request, CancellationToken cancellationToken = default);
    Task<List<ReviewDto>> GetReviewsByUserIdAsync(int userId, int? ratingFilter = null, CancellationToken cancellationToken = default);
    Task<ReviewDto?> GetReviewByIdAsync(int reviewId, CancellationToken cancellationToken = default);
    Task<UserReviewSummaryDto> GetUserReviewSummaryAsync(int userId, CancellationToken cancellationToken = default);
    Task<List<OrderHistoryDto>> GetOrderHistoryAsync(int buyerId, CancellationToken cancellationToken = default);
    Task<bool> CanReviewOrderAsync(int orderId, int userId, CancellationToken cancellationToken = default);
}


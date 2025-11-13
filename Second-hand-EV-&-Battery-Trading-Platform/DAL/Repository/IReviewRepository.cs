using DAL.Models;

namespace DAL.Repository;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(int reviewId, CancellationToken cancellationToken = default);
    Task<List<Review>> GetByReviewedUserIdAsync(int reviewedUserId, CancellationToken cancellationToken = default);
    Task<List<Review>> GetByReviewerIdAsync(int reviewerId, CancellationToken cancellationToken = default);
    Task<List<Review>> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);
    Task<Review?> GetByOrderAndReviewerAsync(int orderId, int reviewerId, CancellationToken cancellationToken = default);
    Task<List<Review>> GetByRatingAsync(int reviewedUserId, int? rating, CancellationToken cancellationToken = default);
    Task AddAsync(Review review, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(Review review, CancellationToken cancellationToken = default);
}


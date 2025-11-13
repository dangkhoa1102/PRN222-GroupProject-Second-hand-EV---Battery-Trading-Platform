using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class ReviewRepository : IReviewRepository
{
    private readonly EVTradingPlatformContext _context;

    public ReviewRepository(EVTradingPlatformContext context)
    {
        _context = context;
    }

    public Task<Review?> GetByIdAsync(int reviewId, CancellationToken cancellationToken = default)
    {
        return _context.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.ReviewedUser)
            .Include(r => r.Order)
                .ThenInclude(o => o.VehicleOrder)
                    .ThenInclude(vo => vo.Vehicle)
            .Include(r => r.Order)
                .ThenInclude(o => o.BatteryOrder)
                    .ThenInclude(bo => bo.Battery)
            .FirstOrDefaultAsync(r => r.ReviewId == reviewId, cancellationToken);
    }

    public Task<List<Review>> GetByReviewedUserIdAsync(int reviewedUserId, CancellationToken cancellationToken = default)
    {
        return _context.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.ReviewedUser)
            .Include(r => r.Order)
                .ThenInclude(o => o.VehicleOrder)
                    .ThenInclude(vo => vo.Vehicle)
            .Include(r => r.Order)
                .ThenInclude(o => o.BatteryOrder)
                    .ThenInclude(bo => bo.Battery)
            .Where(r => r.ReviewedUserId == reviewedUserId)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Review>> GetByReviewerIdAsync(int reviewerId, CancellationToken cancellationToken = default)
    {
        return _context.Reviews
            .Include(r => r.ReviewedUser)
            .Include(r => r.Order)
                .ThenInclude(o => o.VehicleOrder)
                    .ThenInclude(vo => vo.Vehicle)
            .Include(r => r.Order)
                .ThenInclude(o => o.BatteryOrder)
                    .ThenInclude(bo => bo.Battery)
            .Where(r => r.ReviewerId == reviewerId)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Review>> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        return _context.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.ReviewedUser)
            .Where(r => r.OrderId == orderId)
            .ToListAsync(cancellationToken);
    }

    public Task<Review?> GetByOrderAndReviewerAsync(int orderId, int reviewerId, CancellationToken cancellationToken = default)
    {
        return _context.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.ReviewedUser)
            .Include(r => r.Order)
            .FirstOrDefaultAsync(r => r.OrderId == orderId && r.ReviewerId == reviewerId, cancellationToken);
    }

    public Task<List<Review>> GetByRatingAsync(int reviewedUserId, int? rating, CancellationToken cancellationToken = default)
    {
        var query = _context.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.Order)
                .ThenInclude(o => o.VehicleOrder)
                    .ThenInclude(vo => vo.Vehicle)
            .Include(r => r.Order)
                .ThenInclude(o => o.BatteryOrder)
                    .ThenInclude(bo => bo.Battery)
            .Where(r => r.ReviewedUserId == reviewedUserId);

        if (rating.HasValue)
        {
            query = query.Where(r => r.Rating == rating.Value);
        }

        return query
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Review review, CancellationToken cancellationToken = default)
    {
        return _context.Reviews.AddAsync(review, cancellationToken).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public Task DeleteAsync(Review review, CancellationToken cancellationToken = default)
    {
        _context.Reviews.Remove(review);
        return Task.CompletedTask;
    }
}


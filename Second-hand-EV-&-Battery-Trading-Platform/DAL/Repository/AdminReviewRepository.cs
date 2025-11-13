using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace DAL.Repositories.Admin
{
    public class AdminReviewRepository : IAdminReviewRepository
    {
        private readonly EVTradingPlatformContext _context;

        public AdminReviewRepository(EVTradingPlatformContext context)
        {
            _context = context;
        }

        public async Task<List<Review>> GetAllReviewsAsync()
        {
            return await _context.Reviews
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task DeleteReviewAsync(int reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
            }
        }
    }
}

using BLL.DTOs;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services
{
    public class AdminReviewService : IAdminReviewService
    {
        private readonly EVTradingPlatformContext _context;

        public AdminReviewService(EVTradingPlatformContext context)
        {
            _context = context;
        }

        public async Task<List<AdminReviewDto>> GetAllReviewsAsync()
        {
            var query = from r in _context.Reviews
                        join reviewer in _context.Users on r.ReviewerId equals reviewer.UserId
                        join reviewed in _context.Users on r.ReviewedUserId equals reviewed.UserId
                        select new AdminReviewDto
                        {
                            ReviewId = r.ReviewId,
                            OrderId = r.OrderId,
                            ReviewerName = reviewer.FullName,
                            ReviewedUserName = reviewed.FullName,
                            Rating = r.Rating,
                            Comment = r.Comment,
                            CreatedDate = r.CreatedDate
                        };

            return await query
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }
    }
}

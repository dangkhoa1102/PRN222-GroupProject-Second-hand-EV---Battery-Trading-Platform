using BLL.DTOs;
using DAL.Models;
using DAL.Repository;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class ReviewService : IReviewService
{
    public async Task<ReviewResultDto> CreateReviewAsync(int reviewerId, CreateReviewDto request, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var orderRepository = new OrderRepository(context);
        var reviewRepository = new ReviewRepository(context);

        // Kiểm tra đơn hàng có tồn tại không
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            return ReviewResultDto.Failure("Đơn hàng không tồn tại.");
        }

        // Kiểm tra user có phải là buyer không
        if (order.BuyerId != reviewerId)
        {
            return ReviewResultDto.Failure("Bạn không có quyền đánh giá đơn hàng này.");
        }

        // Kiểm tra đơn hàng đã hoàn thành chưa
        if (!order.CompletedDate.HasValue)
        {
            return ReviewResultDto.Failure("Chỉ có thể đánh giá đơn hàng đã hoàn thành.");
        }

        // Kiểm tra đã review chưa
        var existingReview = await reviewRepository.GetByOrderAndReviewerAsync(request.OrderId, reviewerId, cancellationToken);
        if (existingReview != null)
        {
            return ReviewResultDto.Failure("Bạn đã đánh giá đơn hàng này rồi.");
        }

        // Tạo review mới
        var review = new Review
        {
            OrderId = request.OrderId,
            ReviewerId = reviewerId,
            ReviewedUserId = order.SellerId,
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedDate = DateTime.UtcNow
        };

        await reviewRepository.AddAsync(review, cancellationToken);
        await reviewRepository.SaveChangesAsync(cancellationToken);

        return ReviewResultDto.Success(review.ReviewId);
    }

    public async Task<List<ReviewDto>> GetReviewsByUserIdAsync(int userId, int? ratingFilter = null, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var reviewRepository = new ReviewRepository(context);

        var reviews = await reviewRepository.GetByRatingAsync(userId, ratingFilter, cancellationToken);

        return reviews.Select(MapToReviewDto).ToList();
    }

    public async Task<ReviewDto?> GetReviewByIdAsync(int reviewId, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var reviewRepository = new ReviewRepository(context);

        var review = await reviewRepository.GetByIdAsync(reviewId, cancellationToken);
        return review == null ? null : MapToReviewDto(review);
    }

    public async Task<UserReviewSummaryDto> GetUserReviewSummaryAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var reviewRepository = new ReviewRepository(context);

        var reviews = await reviewRepository.GetByReviewedUserIdAsync(userId, cancellationToken);

        // Tính toán thống kê
        var totalReviews = reviews.Count;
        if (totalReviews == 0)
        {
            // Nếu chưa có review, lấy tên user từ database
            var allUsers = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

            return new UserReviewSummaryDto
            {
                UserId = userId,
                UserName = allUsers?.FullName ?? "Unknown",
                AverageRating = 0,
                TotalReviews = 0
            };
        }

        var rating1Count = reviews.Count(r => r.Rating == 1);
        var rating2Count = reviews.Count(r => r.Rating == 2);
        var rating3Count = reviews.Count(r => r.Rating == 3);
        var rating4Count = reviews.Count(r => r.Rating == 4);
        var rating5Count = reviews.Count(r => r.Rating == 5);

        var averageRating = reviews.Average(r => (double)r.Rating);

        // Lấy tên user từ review đầu tiên hoặc từ database
        var userName = reviews.FirstOrDefault()?.ReviewedUser?.FullName;
        
        // Nếu không lấy được từ review, lấy trực tiếp từ database
        if (string.IsNullOrWhiteSpace(userName))
        {
            var user = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
            userName = user?.FullName ?? "Unknown";
        }

        return new UserReviewSummaryDto
        {
            UserId = userId,
            UserName = userName,
            AverageRating = Math.Round(averageRating, 2),
            TotalReviews = totalReviews,
            Rating1Count = rating1Count,
            Rating2Count = rating2Count,
            Rating3Count = rating3Count,
            Rating4Count = rating4Count,
            Rating5Count = rating5Count
        };
    }

    public async Task<List<OrderHistoryDto>> GetOrderHistoryAsync(int buyerId, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var orderRepository = new OrderRepository(context);
        var reviewRepository = new ReviewRepository(context);

        var orders = await orderRepository.GetCompletedOrdersByBuyerIdAsync(buyerId, cancellationToken);

        var result = new List<OrderHistoryDto>();

        foreach (var order in orders)
        {
            var review = await reviewRepository.GetByOrderAndReviewerAsync(order.OrderId, buyerId, cancellationToken);

            string? productType = null;
            string? productName = null;
            int? productId = null;

            if (order.VehicleOrder != null)
            {
                productType = "Vehicle";
                productName = $"{order.VehicleOrder.Vehicle?.Brand} {order.VehicleOrder.Vehicle?.Model}";
                productId = order.VehicleOrder.VehicleId;
            }
            else if (order.BatteryOrder != null)
            {
                productType = "Battery";
                productName = $"{order.BatteryOrder.Battery?.Brand} {order.BatteryOrder.Battery?.BatteryType}";
                productId = order.BatteryOrder.BatteryId;
            }

            result.Add(new OrderHistoryDto
            {
                OrderId = order.OrderId,
                SellerId = order.SellerId,
                SellerName = order.Seller?.FullName ?? string.Empty,
                SellerEmail = order.Seller?.Email,
                TotalAmount = order.TotalAmount,
                PaymentMethod = order.PaymentMethod,
                CreatedDate = order.CreatedDate,
                CompletedDate = order.CompletedDate,
                ProductType = productType,
                ProductName = productName,
                ProductId = productId,
                HasReview = review != null,
                ReviewId = review?.ReviewId
            });
        }

        return result;
    }

    public async Task<bool> CanReviewOrderAsync(int orderId, int userId, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var orderRepository = new OrderRepository(context);

        return await orderRepository.CanReviewOrderAsync(orderId, userId, cancellationToken);
    }

    private static ReviewDto MapToReviewDto(Review review)
    {
        string? productType = null;
        string? productName = null;
        int? productId = null;

        if (review.Order?.VehicleOrder != null)
        {
            productType = "Vehicle";
            productName = $"{review.Order.VehicleOrder.Vehicle?.Brand} {review.Order.VehicleOrder.Vehicle?.Model}";
            productId = review.Order.VehicleOrder.VehicleId;
        }
        else if (review.Order?.BatteryOrder != null)
        {
            productType = "Battery";
            productName = $"{review.Order.BatteryOrder.Battery?.Brand} {review.Order.BatteryOrder.Battery?.BatteryType}";
            productId = review.Order.BatteryOrder.BatteryId;
        }

        return new ReviewDto
        {
            ReviewId = review.ReviewId,
            OrderId = review.OrderId,
            ReviewerId = review.ReviewerId,
            ReviewerName = review.Reviewer?.FullName ?? string.Empty,
            ReviewerEmail = review.Reviewer?.Email,
            ReviewedUserId = review.ReviewedUserId,
            ReviewedUserName = review.ReviewedUser?.FullName ?? string.Empty,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedDate = review.CreatedDate,
            ProductType = productType,
            ProductName = productName,
            ProductId = productId
        };
    }
}


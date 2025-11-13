using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class OrderRepository : IOrderRepository
{
    private readonly EVTradingPlatformContext _context;

    public OrderRepository(EVTradingPlatformContext context)
    {
        _context = context;
    }

    // Methods from main branch
    public Task<Order?> GetByIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        return _context.Orders
            .Include(o => o.Buyer)
            .Include(o => o.Seller)
            .Include(o => o.VehicleOrder)
                .ThenInclude(vo => vo.Vehicle)
            .Include(o => o.BatteryOrder)
                .ThenInclude(bo => bo.Battery)
            .Include(o => o.Reviews)
            .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
    }

    public Task<List<Order>> GetByBuyerIdAsync(int buyerId, CancellationToken cancellationToken = default)
    {
        return _context.Orders
            .Include(o => o.Seller)
            .Include(o => o.VehicleOrder)
                .ThenInclude(vo => vo.Vehicle)
            .Include(o => o.BatteryOrder)
                .ThenInclude(bo => bo.Battery)
            .Include(o => o.Reviews)
            .Where(o => o.BuyerId == buyerId)
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Order>> GetBySellerIdAsync(int sellerId, CancellationToken cancellationToken = default)
    {
        return _context.Orders
            .Include(o => o.Buyer)
            .Include(o => o.VehicleOrder)
                .ThenInclude(vo => vo.Vehicle)
            .Include(o => o.BatteryOrder)
                .ThenInclude(bo => bo.Battery)
            .Include(o => o.Reviews)
            .Where(o => o.SellerId == sellerId)
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Order>> GetCompletedOrdersByBuyerIdAsync(int buyerId, CancellationToken cancellationToken = default)
    {
        return _context.Orders
            .Include(o => o.Seller)
            .Include(o => o.VehicleOrder)
                .ThenInclude(vo => vo.Vehicle)
            .Include(o => o.BatteryOrder)
                .ThenInclude(bo => bo.Battery)
            .Include(o => o.Reviews)
            .Where(o => o.BuyerId == buyerId && o.CompletedDate.HasValue)
            .OrderByDescending(o => o.CompletedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CanReviewOrderAsync(int orderId, int userId, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.Reviews)
            .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);

        if (order == null || !order.CompletedDate.HasValue)
        {
            return false;
        }

        // Kiểm tra xem user có phải là buyer không
        if (order.BuyerId != userId)
        {
            return false;
        }

        // Kiểm tra xem đã review chưa
        var existingReview = order.Reviews.FirstOrDefault(r => r.ReviewerId == userId && r.ReviewedUserId == order.SellerId);
        return existingReview == null;
    }

    // Methods from HEAD branch (seller-specific)
    public Task<List<Order>> GetSellerOrdersAsync(int sellerId, CancellationToken cancellationToken = default)
    {
        return _context.Orders
            .Include(o => o.Buyer)
            .Include(o => o.VehicleOrder)
                .ThenInclude(vo => vo.Vehicle)
            .Include(o => o.BatteryOrder)
                .ThenInclude(bo => bo.Battery)
            .Where(o => o.SellerId == sellerId)
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public Task<Order?> GetOrderDetailAsync(int orderId, int sellerId, CancellationToken cancellationToken = default)
    {
        return _context.Orders
            .Include(o => o.Buyer)
            .Include(o => o.VehicleOrder)
                .ThenInclude(vo => vo.Vehicle)
            .Include(o => o.BatteryOrder)
                .ThenInclude(bo => bo.Battery)
            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.SellerId == sellerId, cancellationToken);
    }

    public Task UpdateOrderAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Update(order);
        return Task.CompletedTask;
    }

    public Task DeleteOrderAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Remove(order);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}

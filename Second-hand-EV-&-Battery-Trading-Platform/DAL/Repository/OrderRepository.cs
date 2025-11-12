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

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}

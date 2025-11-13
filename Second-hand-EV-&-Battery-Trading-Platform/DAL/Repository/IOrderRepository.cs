using DAL.Models;

namespace DAL.Repository;

public interface IOrderRepository
{
    // Methods from main branch
    Task<Order?> GetByIdAsync(int orderId, CancellationToken cancellationToken = default);
    Task<List<Order>> GetByBuyerIdAsync(int buyerId, CancellationToken cancellationToken = default);
    Task<List<Order>> GetBySellerIdAsync(int sellerId, CancellationToken cancellationToken = default);
    Task<List<Order>> GetCompletedOrdersByBuyerIdAsync(int buyerId, CancellationToken cancellationToken = default);
    Task<bool> CanReviewOrderAsync(int orderId, int userId, CancellationToken cancellationToken = default);
    
    // Methods from HEAD branch (seller-specific)
    Task<List<Order>> GetSellerOrdersAsync(int sellerId, CancellationToken cancellationToken = default);
    Task<Order?> GetOrderDetailAsync(int orderId, int sellerId, CancellationToken cancellationToken = default);
    Task UpdateOrderAsync(Order order, CancellationToken cancellationToken = default);
    Task DeleteOrderAsync(Order order, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

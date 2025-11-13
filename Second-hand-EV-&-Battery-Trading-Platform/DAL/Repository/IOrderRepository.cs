using DAL.Models;

namespace DAL.Repository;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int orderId, CancellationToken cancellationToken = default);
    Task<List<Order>> GetByBuyerIdAsync(int buyerId, CancellationToken cancellationToken = default);
    Task<List<Order>> GetBySellerIdAsync(int sellerId, CancellationToken cancellationToken = default);
    Task<List<Order>> GetCompletedOrdersByBuyerIdAsync(int buyerId, CancellationToken cancellationToken = default);
    Task<bool> CanReviewOrderAsync(int orderId, int userId, CancellationToken cancellationToken = default);
}


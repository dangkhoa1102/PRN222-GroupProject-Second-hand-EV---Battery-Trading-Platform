using DAL.Models;

namespace DAL.Repository;

public interface IOrderRepository
{
    Task<List<Order>> GetSellerOrdersAsync(int sellerId, CancellationToken cancellationToken = default);
    Task<Order?> GetOrderDetailAsync(int orderId, int sellerId, CancellationToken cancellationToken = default);
    Task UpdateOrderAsync(Order order, CancellationToken cancellationToken = default);
    Task DeleteOrderAsync(Order order, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

using BLL.DTOs;

namespace BLL.Services;

public interface IOrderService
{
    Task<List<OrderListDto>> GetSellerOrdersAsync(int sellerId, CancellationToken cancellationToken = default);
    Task<OrderDetailDto?> GetOrderDetailAsync(int orderId, int sellerId, CancellationToken cancellationToken = default);
    Task<bool> ConfirmOrderAsync(int orderId, int sellerId, CancellationToken cancellationToken = default);
    Task<bool> RejectOrderAsync(int orderId, int sellerId, string? reason, CancellationToken cancellationToken = default);
}


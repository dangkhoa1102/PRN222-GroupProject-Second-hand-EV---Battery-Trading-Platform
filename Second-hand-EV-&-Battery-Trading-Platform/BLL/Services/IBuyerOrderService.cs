using BLL.DTOs;

namespace BLL.Services;

public interface IBuyerOrderService
{
    Task<List<OrderListDto>> GetBuyerOrdersAsync(int buyerId, CancellationToken cancellationToken = default);
    Task<OrderDetailDto?> GetBuyerOrderDetailAsync(int orderId, int buyerId, CancellationToken cancellationToken = default);
    Task<bool> MarkAsPaidAsync(int orderId, int buyerId, string paymentMethod, CancellationToken cancellationToken = default);
    Task<bool> ConfirmDeliveryAsync(int orderId, int buyerId, CancellationToken cancellationToken = default);
    Task<bool> CancelOrderAsync(int orderId, int buyerId, string? reason, CancellationToken cancellationToken = default);
}


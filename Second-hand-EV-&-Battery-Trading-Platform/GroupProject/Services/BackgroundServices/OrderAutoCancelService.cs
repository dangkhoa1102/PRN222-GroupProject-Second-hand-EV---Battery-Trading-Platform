using DAL.Models;
using DAL.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GroupProject.Services.BackgroundServices;

/// <summary>
/// Background service để tự động hủy đơn hàng sau 1 phút (test) / 24h (production) nếu buyer không nhận hàng
/// Service này chạy định kỳ mỗi 30 giây (test) / 1 giờ (production) để kiểm tra các đơn hàng
/// </summary>
public class OrderAutoCancelService : BackgroundService
{
    private readonly ILogger<OrderAutoCancelService> _logger;
    // private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Kiểm tra mỗi 1 giờ (production)
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30); // Kiểm tra mỗi 30 giây (để test nhanh hơn)
    
    // private readonly TimeSpan _cancelAfterHours = TimeSpan.FromHours(24); // Hủy sau 24h (production - đã comment)
    private readonly TimeSpan _cancelAfterHours = TimeSpan.FromMinutes(1); // Hủy sau 1 phút (để test)

    public OrderAutoCancelService(ILogger<OrderAutoCancelService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OrderAutoCancelService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOrdersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing orders for auto-cancel.");
            }

            // Đợi interval trước khi chạy lại
            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("OrderAutoCancelService is stopping.");
    }

    private async Task ProcessOrdersAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var context = new EVTradingPlatformContext();
            var orderRepository = new OrderRepository(context);

            // Lấy tất cả các đơn hàng có trạng thái "Confirmed" (seller đã xác nhận)
            // và đã được confirm hơn 1 phút (hoặc 24h trong production) nhưng chưa có DeliveryDate
            var cutoffTime = DateTime.Now.Subtract(_cancelAfterHours);
            var ordersToCancel = await context.Orders
                .Where(o => o.OrderStatus == "Confirmed" 
                         && o.CompletedDate.HasValue 
                         && o.CompletedDate.Value <= cutoffTime
                         && !o.DeliveryDate.HasValue)
                .ToListAsync(cancellationToken);

            if (!ordersToCancel.Any())
            {
                _logger.LogDebug("No orders found to auto-cancel.");
                return;
            }

            var cancelledCount = 0;
            var now = DateTime.Now;

            foreach (var order in ordersToCancel)
            {
                try
                {
                    // Tự động hủy đơn hàng
                    order.OrderStatus = "Cancelled";
                    // order.CancellationReason = "Người mua không nhận hàng sau 24 giờ"; // Production
                    order.CancellationReason = "Người mua không nhận hàng sau 1 phút"; // Test

                    await orderRepository.UpdateOrderAsync(order);
                    await orderRepository.SaveChangesAsync(cancellationToken);

                    cancelledCount++;
                    _logger.LogInformation(
                        "Order {OrderId} has been auto-cancelled. Confirmed at: {ConfirmedDate}, Cancelled at: {CancelledDate}",
                        order.OrderId,
                        order.CompletedDate,
                        now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cancelling order {OrderId}.", order.OrderId);
                }
            }

            if (cancelledCount > 0)
            {
                _logger.LogInformation("Auto-cancelled {Count} orders that were not delivered within {Hours} minutes.", cancelledCount, _cancelAfterHours.TotalMinutes);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing orders for auto-cancel.");
            // Không throw exception để service vẫn tiếp tục chạy
        }
    }
}


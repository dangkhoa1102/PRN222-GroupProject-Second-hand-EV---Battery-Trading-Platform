using BLL.Services;

namespace GroupProject.Services.BackgroundServices;

/// <summary>
/// Background service để tự động hủy đơn hàng sau 5 phút nếu buyer không xác nhận nhận hàng
/// Service này chạy định kỳ mỗi 30 giây để kiểm tra các đơn hàng có trạng thái "Delivered"
/// và đã được giao hơn 5 phút nhưng buyer chưa xác nhận nhận hàng
/// </summary>
public class OrderAutoCancelService : BackgroundService
{
    private readonly ILogger<OrderAutoCancelService> _logger;
    private readonly IServiceProvider _serviceProvider;
    // private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Kiểm tra mỗi 1 giờ (production)
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30); // Kiểm tra mỗi 30 giây (để test nhanh hơn)
    
    // private readonly TimeSpan _cancelAfterHours = TimeSpan.FromHours(24); // Hủy sau 24h (production - đã comment)
    private readonly TimeSpan _cancelAfterHours = TimeSpan.FromMinutes(5); // Hủy sau 5 phút nếu buyer không xác nhận

    public OrderAutoCancelService(ILogger<OrderAutoCancelService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
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
            using var scope = _serviceProvider.CreateScope();
            var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var cancelledOrders = await orderService.AutoCancelDeliveredOrdersAsync(_cancelAfterHours, cancellationToken);

            if (!cancelledOrders.Any())
            {
                _logger.LogDebug("No orders found to auto-cancel.");
                return;
            }

            foreach (var order in cancelledOrders)
            {
                try
                {
                    await notificationService.NotifyOrderUpdateAsync(
                        order.OrderId,
                        order.SellerId,
                        order.BuyerId,
                        "Đơn hàng đã bị hủy tự động: Người mua không xác nhận nhận hàng sau thời gian quy định.",
                        "Cancelled");

                    _logger.LogInformation(
                        "Order {OrderId} has been auto-cancelled. Delivered at: {DeliveredDate}, Cancelled at: {CancelledDate}",
                        order.OrderId,
                        order.DeliveryDate,
                        order.CancelledAt);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while sending notification for cancelled order {OrderId}.", order.OrderId);
                }
            }

            _logger.LogInformation("Auto-cancelled {Count} orders that were not confirmed by buyer within {Minutes} minutes.", cancelledOrders.Count, _cancelAfterHours.TotalMinutes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing orders for auto-cancel.");
            // Không throw exception để service vẫn tiếp tục chạy
        }
    }
}


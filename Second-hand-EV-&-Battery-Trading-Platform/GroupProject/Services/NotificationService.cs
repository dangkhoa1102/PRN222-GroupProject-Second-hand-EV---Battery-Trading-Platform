using GroupProject.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace GroupProject.Services;

public interface INotificationService
{
    // Order notifications
    Task NotifyOrderUpdateAsync(int orderId, int sellerId, int buyerId, string message, string orderStatus);
    Task NotifyNewOrderAsync(int orderId, int sellerId, string buyerName, decimal totalAmount);
    
    // Listing notifications
    Task NotifyListingStatusChangeAsync(int listingId, string listingType, int sellerId, string status, string message);
    Task NotifyListingCreatedAsync(int listingId, string listingType, int sellerId, string listingName);
    Task NotifyListingUpdatedAsync(int listingId, string listingType, int sellerId, string listingName);
    Task NotifyListingSubmittedAsync(int listingId, string listingType, int sellerId, string listingName);
    Task NotifyListingApprovedAsync(int listingId, string listingType, int sellerId, string listingName);
    Task NotifyListingRejectedAsync(int listingId, string listingType, int sellerId, string listingName, string reason);
    Task NotifyListingNeedsRevisionAsync(int listingId, string listingType, int sellerId, string listingName, string note);
    Task NotifyListingHiddenAsync(int listingId, string listingType, int sellerId, string listingName);
    Task NotifyListingDeletedAsync(int listingId, string listingType, int sellerId, string listingName);
    
    // Review notifications
    Task NotifyNewReviewAsync(int reviewId, int reviewedUserId, string reviewerName, int rating);
    
    // Admin notifications
    Task NotifyAdminAsync(string message, string type = "info");
    Task NotifyNewPendingListingAsync(int listingId, string listingType, int sellerId, string listingName);
}

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyOrderUpdateAsync(int orderId, int sellerId, int buyerId, string message, string orderStatus)
    {
        // Gửi cho seller
        await _hubContext.Clients.Group($"seller_{sellerId}").SendAsync("OrderUpdated", new
        {
            OrderId = orderId,
            Message = message,
            OrderStatus = orderStatus,
            Timestamp = DateTime.Now
        });

        // Gửi cho buyer
        await _hubContext.Clients.Group($"buyer_{buyerId}").SendAsync("OrderUpdated", new
        {
            OrderId = orderId,
            Message = message,
            OrderStatus = orderStatus,
            Timestamp = DateTime.Now
        });
    }

    public async Task NotifyNewOrderAsync(int orderId, int sellerId, string buyerName, decimal totalAmount)
    {
        await _hubContext.Clients.Group($"seller_{sellerId}").SendAsync("NewOrder", new
        {
            OrderId = orderId > 0 ? orderId : 0,
            BuyerName = buyerName,
            TotalAmount = totalAmount,
            Message = orderId > 0 ? $"Có đơn hàng mới #{orderId}" : "Có đơn hàng mới",
            Timestamp = DateTime.Now
        });
    }

    public async Task NotifyListingStatusChangeAsync(int listingId, string listingType, int sellerId, string status, string message)
    {
        await _hubContext.Clients.Group($"seller_{sellerId}").SendAsync("ListingStatusChanged", new
        {
            ListingId = listingId,
            ListingType = listingType,
            Status = status,
            Message = message,
            Timestamp = DateTime.Now
        });

        // Nếu được approve, thông báo cho admin
        if (status == "Approved")
        {
            await _hubContext.Clients.Group("admin").SendAsync("ListingApproved", new
            {
                ListingId = listingId,
                ListingType = listingType,
                Timestamp = DateTime.Now
            });
        }
    }

    public async Task NotifyNewReviewAsync(int reviewId, int reviewedUserId, string reviewerName, int rating)
    {
        await _hubContext.Clients.Group($"user_{reviewedUserId}").SendAsync("NewReview", new
        {
            ReviewId = reviewId,
            ReviewerName = reviewerName,
            Rating = rating,
            Timestamp = DateTime.Now
        });
    }

    public async Task NotifyListingCreatedAsync(int listingId, string listingType, int sellerId, string listingName)
    {
        // Gửi cho seller group
        await _hubContext.Clients.Group($"seller_{sellerId}").SendAsync("ListingCreated", new
        {
            ListingId = listingId,
            ListingType = listingType,
            ListingName = listingName,
            Message = $"Tin đăng \"{listingName}\" đã được tạo thành công",
            Timestamp = DateTime.Now
        });
        
        // Cũng gửi cho user group
        await _hubContext.Clients.Group($"user_{sellerId}").SendAsync("ListingCreated", new
        {
            ListingId = listingId,
            ListingType = listingType,
            ListingName = listingName,
            Message = $"Tin đăng \"{listingName}\" đã được tạo thành công",
            Timestamp = DateTime.Now
        });
    }

    public async Task NotifyListingUpdatedAsync(int listingId, string listingType, int sellerId, string listingName)
    {
        var ownerPayload = new
        {
            ListingId = listingId,
            ListingType = listingType,
            ListingName = listingName,
            Message = $"Tin đăng \"{listingName}\" đã được cập nhật",
            Timestamp = DateTime.Now
        };

        // Gửi cho seller group
        await _hubContext.Clients.Group($"seller_{sellerId}").SendAsync("ListingUpdated", ownerPayload);
        
        // Cũng gửi cho user group
        await _hubContext.Clients.Group($"user_{sellerId}").SendAsync("ListingUpdated", ownerPayload);

        // Gửi cho toàn bộ client để cập nhật realtime cho người mua
        await _hubContext.Clients.All.SendAsync("ListingPublicUpdated", new
        {
            ListingId = listingId,
            ListingType = listingType,
            ListingName = listingName,
            Timestamp = DateTime.Now
        });
    }

    public async Task NotifyListingHiddenAsync(int listingId, string listingType, int sellerId, string listingName)
    {
        // Gửi cho seller group
        await _hubContext.Clients.Group($"seller_{sellerId}").SendAsync("ListingHidden", new
        {
            ListingId = listingId,
            ListingType = listingType,
            ListingName = listingName,
            Message = $"Tin đăng \"{listingName}\" đã được ẩn",
            Timestamp = DateTime.Now
        });
        
        // Cũng gửi cho user group
        await _hubContext.Clients.Group($"user_{sellerId}").SendAsync("ListingHidden", new
        {
            ListingId = listingId,
            ListingType = listingType,
            ListingName = listingName,
            Message = $"Tin đăng \"{listingName}\" đã được ẩn",
            Timestamp = DateTime.Now
        });
    }

    public async Task NotifyListingSubmittedAsync(int listingId, string listingType, int sellerId, string listingName)
    {
        // Gửi cho seller group
        await _hubContext.Clients.Group($"seller_{sellerId}").SendAsync("ListingSubmitted", new
        {
            ListingId = listingId,
            ListingType = listingType,
            ListingName = listingName,
            Message = $"Tin đăng \"{listingName}\" đã được gửi duyệt",
            Timestamp = DateTime.Now
        });
        
        // Cũng gửi cho user group
        await _hubContext.Clients.Group($"user_{sellerId}").SendAsync("ListingSubmitted", new
        {
            ListingId = listingId,
            ListingType = listingType,
            ListingName = listingName,
            Message = $"Tin đăng \"{listingName}\" đã được gửi duyệt",
            Timestamp = DateTime.Now
        });

        // Thông báo cho admin
        await _hubContext.Clients.Group("admin").SendAsync("NewPendingListing", new
        {
            ListingId = listingId,
            ListingType = listingType,
            SellerId = sellerId,
            ListingName = listingName,
            Message = $"Có tin đăng mới chờ duyệt: {listingName}",
            Timestamp = DateTime.Now
        });
    }

    public async Task NotifyListingApprovedAsync(int listingId, string listingType, int sellerId, string listingName)
    {
        var message = $"Tin đăng \"{listingName}\" đã được phê duyệt";
        var notificationData = new
        {
            ListingId = listingId,
            ListingType = listingType,
            ListingName = listingName,
            Message = message,
            Timestamp = DateTime.Now
        };

        Console.WriteLine($"SignalR: Sending ListingApproved to seller_{sellerId} and user_{sellerId} for listing {listingId}");

        // Gửi cho seller group
        await _hubContext.Clients.Group($"seller_{sellerId}").SendAsync("ListingApproved", notificationData);
        
        // Cũng gửi cho user group để đảm bảo nhận được
        await _hubContext.Clients.Group($"user_{sellerId}").SendAsync("ListingApproved", notificationData);
    }

    public async Task NotifyListingRejectedAsync(int listingId, string listingType, int sellerId, string listingName, string reason)
    {
        var message = $"Tin đăng \"{listingName}\" đã bị từ chối: {reason}";
        var notificationData = new
        {
            ListingId = listingId,
            ListingType = listingType,
            ListingName = listingName,
            Reason = reason,
            Message = message,
            Timestamp = DateTime.Now
        };

        Console.WriteLine($"SignalR: Sending ListingRejected to seller_{sellerId} and user_{sellerId} for listing {listingId}");

        // Gửi cho seller group
        await _hubContext.Clients.Group($"seller_{sellerId}").SendAsync("ListingRejected", notificationData);
        
        // Cũng gửi cho user group để đảm bảo nhận được
        await _hubContext.Clients.Group($"user_{sellerId}").SendAsync("ListingRejected", notificationData);
    }

    public async Task NotifyListingNeedsRevisionAsync(int listingId, string listingType, int sellerId, string listingName, string note)
    {
        var message = $"Tin đăng \"{listingName}\" cần chỉnh sửa: {note}";
        var notificationData = new
        {
            ListingId = listingId,
            ListingType = listingType,
            ListingName = listingName,
            Note = note,
            Message = message,
            Timestamp = DateTime.Now
        };

        Console.WriteLine($"SignalR: Sending ListingNeedsRevision to seller_{sellerId} and user_{sellerId} for listing {listingId}");

        // Gửi cho seller group
        await _hubContext.Clients.Group($"seller_{sellerId}").SendAsync("ListingNeedsRevision", notificationData);
        
        // Cũng gửi cho user group để đảm bảo nhận được
        await _hubContext.Clients.Group($"user_{sellerId}").SendAsync("ListingNeedsRevision", notificationData);
    }

    public async Task NotifyListingDeletedAsync(int listingId, string listingType, int sellerId, string listingName)
    {
        // Gửi cho seller
        await _hubContext.Clients.Group($"seller_{sellerId}").SendAsync("ListingDeleted", new
        {
            ListingId = listingId,
            ListingType = listingType,
            ListingName = listingName,
            Message = $"Tin đăng \"{listingName}\" đã bị xóa",
            Timestamp = DateTime.Now
        });
        
        // Cũng gửi cho user group để đảm bảo nhận được
        await _hubContext.Clients.Group($"user_{sellerId}").SendAsync("ListingDeleted", new
        {
            ListingId = listingId,
            ListingType = listingType,
            ListingName = listingName,
            Message = $"Tin đăng \"{listingName}\" đã bị xóa",
            Timestamp = DateTime.Now
        });
    }

    public async Task NotifyNewPendingListingAsync(int listingId, string listingType, int sellerId, string listingName)
    {
        await _hubContext.Clients.Group("admin").SendAsync("NewPendingListing", new
        {
            ListingId = listingId,
            ListingType = listingType,
            SellerId = sellerId,
            ListingName = listingName,
            Message = $"Có tin đăng mới chờ duyệt: {listingName}",
            Timestamp = DateTime.Now
        });
    }

    public async Task NotifyAdminAsync(string message, string type = "info")
    {
        await _hubContext.Clients.Group("admin").SendAsync("AdminNotification", new
        {
            Message = message,
            Type = type,
            Timestamp = DateTime.Now
        });
    }
}


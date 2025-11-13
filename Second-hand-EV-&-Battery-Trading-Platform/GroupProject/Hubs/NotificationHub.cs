using Microsoft.AspNetCore.SignalR;

namespace GroupProject.Hubs;

public class NotificationHub : Hub
{
    // Kết nối user vào group dựa trên userId
    public async Task JoinUserGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        Console.WriteLine($"SignalR: User {userId} joined user_{userId} group. ConnectionId: {Context.ConnectionId}");
    }

    // Kết nối seller vào group của họ
    public async Task JoinSellerGroup(string sellerId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"seller_{sellerId}");
        Console.WriteLine($"SignalR: Seller {sellerId} joined seller_{sellerId} group. ConnectionId: {Context.ConnectionId}");
    }

    // Kết nối buyer vào group của họ
    public async Task JoinBuyerGroup(string buyerId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"buyer_{buyerId}");
        Console.WriteLine($"SignalR: Buyer {buyerId} joined buyer_{buyerId} group. ConnectionId: {Context.ConnectionId}");
    }

    // Kết nối admin vào group admin
    public async Task JoinAdminGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "admin");
        Console.WriteLine($"SignalR: Admin joined admin group. ConnectionId: {Context.ConnectionId}");
    }

    // Ngắt kết nối
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}


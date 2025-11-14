namespace BLL.DTOs;

public class AdminDashboardStatsDto
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public List<AdminTrendPointDto> MonthlyTrends { get; set; } = new();
    public List<AdminStatusBreakdownDto> StatusBreakdown { get; set; } = new();
}



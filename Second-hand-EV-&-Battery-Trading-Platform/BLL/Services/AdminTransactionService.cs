using BLL.DTOs;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services
{
    public class AdminTransactionService : IAdminTransactionService
    {
        private readonly EVTradingPlatformContext _context;

        public AdminTransactionService(EVTradingPlatformContext context)
        {
            _context = context;
        }

        // ✅ 1. Tất cả giao dịch (Orders)
        public async Task<List<OrderTransactionDto>> GetAllOrdersAsync()
        {
            var query = from o in _context.Orders
                        join buyer in _context.Users on o.BuyerId equals buyer.UserId
                        join seller in _context.Users on o.SellerId equals seller.UserId
                        select new OrderTransactionDto
                        {
                            OrderId = o.OrderId,
                            BuyerName = buyer.FullName,
                            SellerName = seller.FullName,
                            TotalAmount = o.TotalAmount,
                            PaymentMethod = o.PaymentMethod ?? "",
                            CreatedDate = o.CreatedDate,
                            CompletedDate = o.CompletedDate
                        };

            return await query.OrderByDescending(x => x.OrderId).ToListAsync();
        }

        // ✅ 2. Giao dịch xe (Orders + VehicleOrders + Vehicles)
        public async Task<List<VehicleTransactionDto>> GetVehicleTransactionsAsync()
        {
            var query = from o in _context.Orders
                        join vo in _context.VehicleOrders on o.OrderId equals vo.OrderId
                        join v in _context.Vehicles on vo.VehicleId equals v.VehicleId
                        join buyer in _context.Users on o.BuyerId equals buyer.UserId
                        join seller in _context.Users on o.SellerId equals seller.UserId
                        select new VehicleTransactionDto
                        {
                            OrderId = o.OrderId,
                            BuyerName = buyer.FullName,
                            SellerName = seller.FullName,
                            VehicleBrand = v.Brand,
                            VehicleModel = v.Model,
                            TotalAmount = o.TotalAmount,
                            PaymentMethod = o.PaymentMethod ?? "",
                            CompletedDate = o.CompletedDate
                        };

            return await query.OrderByDescending(x => x.CompletedDate).ToListAsync();
        }

        // ✅ 3. Giao dịch pin (Orders + BatteryOrders + Batteries)
        public async Task<List<BatteryTransactionDto>> GetBatteryTransactionsAsync()
        {
            var query = from o in _context.Orders
                        join bo in _context.BatteryOrders on o.OrderId equals bo.OrderId
                        join b in _context.Batteries on bo.BatteryId equals b.BatteryId
                        join buyer in _context.Users on o.BuyerId equals buyer.UserId
                        join seller in _context.Users on o.SellerId equals seller.UserId
                        select new BatteryTransactionDto
                        {
                            OrderId = o.OrderId,
                            BuyerName = buyer.FullName,
                            SellerName = seller.FullName,
                            BatteryBrand = b.Brand,
                            BatteryType = b.BatteryType,
                            TotalAmount = o.TotalAmount,
                            PaymentMethod = o.PaymentMethod ?? "",
                            CompletedDate = o.CompletedDate
                        };

            return await query.OrderByDescending(x => x.CompletedDate).ToListAsync();
        }

        public async Task<AdminDashboardStatsDto> GetDashboardStatsAsync(int months = 6)
        {
            months = Math.Clamp(months, 1, 24);

            var totalOrders = await _context.Orders.CountAsync();
            var pendingOrders = await _context.Orders.CountAsync(o => o.OrderStatus == "Pending");
            var completedOrders = await _context.Orders.CountAsync(o => o.OrderStatus == "Completed");
            var cancelledOrders = await _context.Orders.CountAsync(o => o.OrderStatus == "Cancelled");

            var revenueStatuses = new[] { "Completed", "Delivered", "Paid", "Confirm", "Confirmed" };
            var totalRevenue = await _context.Orders
                .Where(o => revenueStatuses.Contains(o.OrderStatus))
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;
            var orderStatusFilter = new[] { "Completed", "Pending", "Cancelled" };

            var endOfWindow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var startOfWindow = endOfWindow.AddMonths(-(months - 1));

            var monthlyRaw = await _context.Orders
                .Where(o =>
                    o.CreatedDate.HasValue &&
                    o.CreatedDate.Value >= startOfWindow &&
                    o.CreatedDate.Value < endOfWindow.AddMonths(1) &&
                    revenueStatuses.Contains(o.OrderStatus))
                .GroupBy(o => new { o.CreatedDate!.Value.Year, o.CreatedDate!.Value.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count(),
                    Revenue = g.Sum(x => x.TotalAmount)
                })
                .ToListAsync();

            var trendPoints = new List<AdminTrendPointDto>();
            for (var cursor = startOfWindow; cursor <= endOfWindow; cursor = cursor.AddMonths(1))
            {
                var match = monthlyRaw.FirstOrDefault(m => m.Year == cursor.Year && m.Month == cursor.Month);
                trendPoints.Add(new AdminTrendPointDto
                {
                    Year = cursor.Year,
                    Month = cursor.Month,
                    OrderCount = match?.Count ?? 0,
                    Revenue = match?.Revenue ?? 0m
                });
            }

            return new AdminDashboardStatsDto
            {
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                CompletedOrders = completedOrders,
                CancelledOrders = cancelledOrders,
                TotalRevenue = totalRevenue,
                MonthlyTrends = trendPoints,
                StatusBreakdown = await _context.Orders
                    .Where(o => orderStatusFilter.Contains(o.OrderStatus))
                    .GroupBy(o => o.OrderStatus)
                    .Select(g => new AdminStatusBreakdownDto
                    {
                        Status = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync()
            };
        }
    }
}

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
    }
}

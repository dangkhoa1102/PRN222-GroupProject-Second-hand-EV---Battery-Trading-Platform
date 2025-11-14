using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public class BuyerRepository : IBuyerRepository
    {
    public async Task<bool> CreateOrder(Order order, string itemType, int itemId)
    {
        if(order == null)
        {
            throw new ArgumentNullException(nameof(order));
        }
        await using var context = new EVTradingPlatformContext();
        
        // Set order status
        order.OrderStatus = "Pending";
        order.CreatedDate = DateTime.Now;
        
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();
        
        // Create VehicleOrder or BatteryOrder and set IsActive = false for listing
        if (itemType == "Vehicle")
        {
            var vehicle = await context.Vehicles.FindAsync(itemId);
            if (vehicle != null)
            {
                vehicle.IsActive = false;
                vehicle.UpdatedDate = DateTime.Now;
            }
            
            var vehicleOrder = new VehicleOrder
            {
                OrderId = order.OrderId,
                VehicleId = itemId
            };
            await context.VehicleOrders.AddAsync(vehicleOrder);
        }
        else if (itemType == "Battery")
        {
            var battery = await context.Batteries.FindAsync(itemId);
            if (battery != null)
            {
                battery.IsActive = false;
                battery.UpdatedDate = DateTime.Now;
            }
            
            var batteryOrder = new BatteryOrder
            {
                OrderId = order.OrderId,
                BatteryId = itemId
            };
            await context.BatteryOrders.AddAsync(batteryOrder);
        }
        
        if(await context.SaveChangesAsync() > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
        
        public async Task<List<Order>> GetBuyerOrdersAsync(int buyerId)
        {
            await using var context = new EVTradingPlatformContext();
            return await context.Orders
                .Include(o => o.Buyer)
                .Include(o => o.Seller)
                .Include(o => o.VehicleOrder)
                    .ThenInclude(vo => vo.Vehicle)
                .Include(o => o.BatteryOrder)
                    .ThenInclude(bo => bo.Battery)
                .Where(o => o.BuyerId == buyerId)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }
        
        public async Task<Order?> GetBuyerOrderDetailAsync(int orderId, int buyerId)
        {
            await using var context = new EVTradingPlatformContext();
            return await context.Orders
                .Include(o => o.Buyer)
                .Include(o => o.Seller)
                .Include(o => o.VehicleOrder)
                    .ThenInclude(vo => vo.Vehicle)
                .Include(o => o.BatteryOrder)
                    .ThenInclude(bo => bo.Battery)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.BuyerId == buyerId);
        }
        
        public async Task<bool> UpdateOrderAsync(Order order)
        {
            await using var context = new EVTradingPlatformContext();
            // Attach order to context and mark as modified
            var existingOrder = await context.Orders.FindAsync(order.OrderId);
            if (existingOrder == null)
            {
                return false;
            }
            
            // Update properties
            existingOrder.OrderStatus = order.OrderStatus;
            existingOrder.DeliveryDate = order.DeliveryDate;
            existingOrder.CancellationReason = order.CancellationReason;
            existingOrder.CompletedDate = order.CompletedDate;
            
            return await context.SaveChangesAsync() > 0;
        }
        
        public async Task SaveChangesAsync()
        {
            await using var context = new EVTradingPlatformContext();
            await context.SaveChangesAsync();
        }

        public async Task<bool> CreateReview(Review review)
        {
            if(review == null)
            {
                throw new ArgumentNullException(nameof(review));
            }
            await using var context = new EVTradingPlatformContext();
            await context.Reviews.AddAsync(review);
            if(await context.SaveChangesAsync() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<IEnumerable<Review>> GetReview(int reviewedUser)
        {
            await using var context = new EVTradingPlatformContext();
            var rs = await context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.ReviewedUser)
                .Include(r => r.Order)
                    .ThenInclude(o => o.VehicleOrder)
                        .ThenInclude(vo => vo.Vehicle)
                .Include(r => r.Order)
                    .ThenInclude(o => o.BatteryOrder)
                        .ThenInclude(bo => bo.Battery)
                .Where(r => r.ReviewedUserId == reviewedUser)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
            return rs;
        }

        public async Task<IEnumerable<Battery>> SearchBattery(string keyword)
        {
            await using var context = new EVTradingPlatformContext();
            var rs = await context.Batteries.Where(b => b.Brand.Contains(keyword) || b.Capacity.Contains(keyword)).ToListAsync();
            return rs;
        }

        public async Task<IEnumerable<Vehicle>> SearchVehicle(string keyword)
        {
            await using var context = new EVTradingPlatformContext();
            var rs = await context.Vehicles.Where(b => b.Brand.Contains(keyword) || b.Model.Contains(keyword)).ToListAsync();
            return rs;
        }

        public async Task ReactivateListingAsync(int orderId)
        {
            await using var context = new EVTradingPlatformContext();
            
            // Check if order has VehicleOrder
            var vehicleOrder = await context.VehicleOrders
                .Include(vo => vo.Vehicle)
                .FirstOrDefaultAsync(vo => vo.OrderId == orderId);
            
            if (vehicleOrder != null && vehicleOrder.Vehicle != null)
            {
                vehicleOrder.Vehicle.IsActive = true;
                vehicleOrder.Vehicle.UpdatedDate = DateTime.Now;
            }
            else
            {
                // Check if order has BatteryOrder
                var batteryOrder = await context.BatteryOrders
                    .Include(bo => bo.Battery)
                    .FirstOrDefaultAsync(bo => bo.OrderId == orderId);
                
                if (batteryOrder != null && batteryOrder.Battery != null)
                {
                    batteryOrder.Battery.IsActive = true;
                    batteryOrder.Battery.UpdatedDate = DateTime.Now;
                }
            }
            
            await context.SaveChangesAsync();
        }
    }
}

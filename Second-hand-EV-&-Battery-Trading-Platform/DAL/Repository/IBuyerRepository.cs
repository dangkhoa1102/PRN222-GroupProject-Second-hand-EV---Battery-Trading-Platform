using DAL.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public interface IBuyerRepository
    {
        Task<IEnumerable<Vehicle>> SearchVehicle(string keyword);
        Task<IEnumerable<Battery>> SearchBattery(string keyword);
        Task<IEnumerable<Review>> GetReview(int reviewedUser);
        Task<bool> CreateOrder(Order order, string itemType, int itemId);
        Task<bool> CreateReview(Review review);
        Task<List<Order>> GetBuyerOrdersAsync(int buyerId);
        Task<Order?> GetBuyerOrderDetailAsync(int orderId, int buyerId);
        Task<bool> UpdateOrderAsync(Order order);
        Task SaveChangesAsync();
        Task ReactivateListingAsync(int orderId);
    }
}

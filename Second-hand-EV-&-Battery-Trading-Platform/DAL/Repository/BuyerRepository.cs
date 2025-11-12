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
        private readonly EVTradingPlatformContext _context;

        public BuyerRepository(EVTradingPlatformContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateOrder(Order order)
        {
            if(order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }
            await _context.Orders.AddAsync(order);
            if(await _context.SaveChangesAsync() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> CreateReview(Review review)
        {
            if(review == null)
            {
                throw new ArgumentNullException(nameof(review));
            }
            await _context.Reviews.AddAsync(review);
            if(await _context.SaveChangesAsync() > 0)
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
            var rs = await _context.Reviews.Where(r => r.ReviewedUserId == reviewedUser).ToListAsync();
            return rs;
        }

        public async Task<IEnumerable<Battery>> SearchBattery(string keyword)
        {
            var rs = await _context.Batteries.Where(b => b.Brand.Contains(keyword) || b.Capacity.Contains(keyword)).ToListAsync();
            return rs;
        }

        public async Task<IEnumerable<Vehicle>> SearchVehicle(string keyword)
        {
            var rs = await _context.Vehicles.Where(b => b.Brand.Contains(keyword) || b.Model.Contains(keyword)).ToListAsync();
            return rs;
        }
    }
}

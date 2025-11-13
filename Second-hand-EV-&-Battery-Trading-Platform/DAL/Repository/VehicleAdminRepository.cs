using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository
{
    public class VehicleAdminRepository : IVehicleAdminRepository
    {
        private readonly EVTradingPlatformContext _context;

        public VehicleAdminRepository(EVTradingPlatformContext context)
        {
            _context = context;
        }

        public async Task<List<Vehicle>> GetAllAsync()
        {
            return await _context.Vehicles
                .Include(v => v.Seller)
                .OrderByDescending(v => v.CreatedDate)
                .ToListAsync();
        }
    }
}

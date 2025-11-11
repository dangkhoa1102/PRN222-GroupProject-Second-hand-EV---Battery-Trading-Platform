using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class VehicleRepository : IVehicleRepository
{
    private readonly EVTradingPlatformContext _context;

    public VehicleRepository(EVTradingPlatformContext context)
    {
        _context = context;
    }

    public Task<Vehicle?> GetByIdAsync(int vehicleId, CancellationToken cancellationToken = default)
    {
        return _context.Vehicles
            .Include(v => v.Seller)
            .Include(v => v.ApprovedByNavigation)
            .FirstOrDefaultAsync(v => v.VehicleId == vehicleId, cancellationToken);
    }

    public Task<List<Vehicle>> GetBySellerAsync(int sellerId, CancellationToken cancellationToken = default)
    {
        return _context.Vehicles
            .Where(v => v.SellerId == sellerId && (v.IsActive ?? true))
            .OrderByDescending(v => v.UpdatedDate ?? v.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Vehicle>> GetByStatusesAsync(IEnumerable<string> statuses, CancellationToken cancellationToken = default)
    {
        var statusList = statuses.ToList();
        return _context.Vehicles
            .Where(v => statusList.Contains(v.Status) && (v.IsActive ?? true))
            .OrderBy(v => v.SubmittedDate)
            .ThenBy(v => v.CreatedDate)
            .Include(v => v.Seller)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Vehicle>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _context.Vehicles
            .Include(v => v.Seller)
            .OrderByDescending(v => v.CreatedDate)
            .ThenByDescending(v => v.SubmittedDate)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        return _context.Vehicles.AddAsync(vehicle, cancellationToken).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}


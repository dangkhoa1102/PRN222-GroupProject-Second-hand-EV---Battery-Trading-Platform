using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class BatteryRepository : IBatteryRepository
{
    private readonly EVTradingPlatformContext _context;

    public BatteryRepository(EVTradingPlatformContext context)
    {
        _context = context;
    }

    public Task<Battery?> GetByIdAsync(int batteryId, CancellationToken cancellationToken = default)
    {
        return _context.Batteries
            .Include(b => b.Seller)
            .Include(b => b.ApprovedByNavigation)
            .FirstOrDefaultAsync(b => b.BatteryId == batteryId, cancellationToken);
    }

    public Task<List<Battery>> GetBySellerAsync(int sellerId, CancellationToken cancellationToken = default)
    {
        return _context.Batteries
            .Where(b => b.SellerId == sellerId && (b.IsActive ?? true))
            .OrderByDescending(b => b.UpdatedDate ?? b.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Battery>> GetByStatusesAsync(IEnumerable<string> statuses, CancellationToken cancellationToken = default)
    {
        var statusList = statuses.ToList();
        return _context.Batteries
            .Where(b => statusList.Contains(b.Status) && (b.IsActive ?? true))
            .OrderBy(b => b.SubmittedDate)
            .ThenBy(b => b.CreatedDate)
            .Include(b => b.Seller)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Battery>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _context.Batteries
            .Include(b => b.Seller)
            .OrderByDescending(b => b.CreatedDate)
            .ThenByDescending(b => b.SubmittedDate)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Battery battery, CancellationToken cancellationToken = default)
    {
        return _context.Batteries.AddAsync(battery, cancellationToken).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}


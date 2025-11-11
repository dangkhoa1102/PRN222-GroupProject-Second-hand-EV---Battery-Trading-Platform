using DAL.Models;

namespace DAL.Repository;

public interface IBatteryRepository
{
    Task<Battery?> GetByIdAsync(int batteryId, CancellationToken cancellationToken = default);
    Task<List<Battery>> GetBySellerAsync(int sellerId, CancellationToken cancellationToken = default);
    Task<List<Battery>> GetByStatusesAsync(IEnumerable<string> statuses, CancellationToken cancellationToken = default);
    Task<List<Battery>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Battery battery, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}


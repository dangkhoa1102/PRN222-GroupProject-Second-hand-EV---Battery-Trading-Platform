using DAL.Models;

namespace DAL.Repository;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(int vehicleId, CancellationToken cancellationToken = default);
    Task<List<Vehicle>> GetBySellerAsync(int sellerId, CancellationToken cancellationToken = default);
    Task<List<Vehicle>> GetByStatusesAsync(IEnumerable<string> statuses, CancellationToken cancellationToken = default);
    Task<List<Vehicle>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}


using DAL.Models;

public interface IVehicleAdminRepository
{
    Task<List<Vehicle>> GetAllAsync();
}

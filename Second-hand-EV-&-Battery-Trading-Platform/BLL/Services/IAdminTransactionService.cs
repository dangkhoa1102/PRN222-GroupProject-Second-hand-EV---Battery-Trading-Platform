using BLL.DTOs;

namespace BLL.Services
{
    public interface IAdminTransactionService
    {
        Task<List<OrderTransactionDto>> GetAllOrdersAsync();
        Task<List<VehicleTransactionDto>> GetVehicleTransactionsAsync();
        Task<List<BatteryTransactionDto>> GetBatteryTransactionsAsync();
    }
}

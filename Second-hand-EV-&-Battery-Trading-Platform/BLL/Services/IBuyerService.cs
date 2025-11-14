using BLL.DTOs;
using DAL.Models;

namespace BLL.Services
{
    public interface IBuyerService
    {
        Task<IEnumerable<Vehicle>> SearchVehicle(string keyword);
        Task<IEnumerable<Battery>> SearchBattery(string keyword);
        Task<IEnumerable<ReviewDto>> GetReview(int reviewedUser);
        Task<bool> CreateOrder(CreateOrderDto order);
        Task<bool> CreateReview(CreateReviewDto review);
    }
}

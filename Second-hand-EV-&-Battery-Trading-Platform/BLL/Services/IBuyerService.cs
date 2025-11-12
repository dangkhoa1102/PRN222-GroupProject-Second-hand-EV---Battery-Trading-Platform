using BLL.DTOs;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IBuyerService
    {
        Task<IEnumerable<Vehicle>> SearchVehicle(string keyword);
        Task<IEnumerable<Battery>> SearchBattery(string keyword);
        Task<IEnumerable<Review>> GetReview(int reviewedUser);
        Task<bool> CreateOrder(CreateOrderDto order);
        Task<bool> CreateReview(CreateReviewDto review);
    }
}

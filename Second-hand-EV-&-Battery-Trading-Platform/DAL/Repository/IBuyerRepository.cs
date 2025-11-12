using DAL.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public interface IBuyerRepository
    {
        Task<IEnumerable<Vehicle>> SearchVehicle(string keyword);
        Task<IEnumerable<Battery>> SearchBattery(string keyword);
        Task<IEnumerable<Review>> GetReview(int reviewedUser);
        Task<bool> CreateOrder(Order order);
        Task<bool> CreateReview(Review review);
    }
}

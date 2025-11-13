using BLL.DTOs;
using DAL.Models;
using DAL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class BuyerService : IBuyerService
    {
        public async Task<bool> CreateOrder(CreateOrderDto order)
        {
            try
            {
                if(order == null)
                {
                    throw new Exception("Order is null");
                }
                if(string.IsNullOrEmpty(order.ItemType) || order.ItemId <= 0)
                {
                    throw new Exception("Item type and Item ID are required");
                }
                
                var orderEntity = new Order
                {
                    BuyerId = order.BuyerId,
                    SellerId = order.SellerId,
                    TotalAmount = order.TotalAmount,
                    PaymentMethod = order.PaymentMethod ?? "Cash",
                    CreatedDate = DateTime.Now,
                    OrderStatus = "Pending"
                };
                
                var repo = new BuyerRepository();
                var result = await repo.CreateOrder(orderEntity, order.ItemType, order.ItemId);
                if(!result)
                {
                    throw new Exception("Failed to create order");
                }
                else
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> CreateReview(CreateReviewDto review)
        {
            try
            {
                if(review == null)
                {
                    throw new Exception("Order is null");
                }
                var reviewEntity = new Review
                {
                    OrderId = review.OrderId,
                    ReviewerId = review.ReviewerId,
                    ReviewedUserId = review.ReviewedUserId,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedDate = DateTime.Now,
                };
                var repo = new BuyerRepository();
                var result = await repo.CreateReview(reviewEntity);
                if(!result)
                {
                    throw new Exception("Failed to create review");
                }
                else
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<Review>> GetReview(int reviewedUser)
        {
            try
            {
                var repo = new BuyerRepository();
                var result = await repo.GetReview(reviewedUser);
                return result ?? new List<Review>();
            }
            catch
            {
                // Trả về empty list nếu có lỗi thay vì throw exception
                return new List<Review>();
            }
        }

        public async Task<IEnumerable<Battery>> SearchBattery(string keyword)
        {
            try
            {
                var repo = new BuyerRepository();
                var result = await repo.SearchBattery(keyword);
                if(result == null)
                {
                    throw new Exception("No batteries found");
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<Vehicle>> SearchVehicle(string keyword)
        {
            try
            {
                var repo = new BuyerRepository();
                var result = await repo.SearchVehicle(keyword);
                if (result == null)
                {
                    throw new Exception("No vehicles found");
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

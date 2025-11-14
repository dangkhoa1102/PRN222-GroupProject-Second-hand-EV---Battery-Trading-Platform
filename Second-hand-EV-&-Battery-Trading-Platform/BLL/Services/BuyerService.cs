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
                    PaymentMethod = null, // Sẽ được set khi buyer chuyển tiền
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

        public async Task<IEnumerable<ReviewDto>> GetReview(int reviewedUser)
        {
            try
            {
                var repo = new BuyerRepository();
                var result = await repo.GetReview(reviewedUser);

                if (result == null)
                {
                    return new List<ReviewDto>();
                }

                return result.Select(review => new ReviewDto
                {
                    ReviewId = review.ReviewId,
                    OrderId = review.OrderId,
                    ReviewerId = review.ReviewerId,
                    ReviewerName = review.Reviewer?.FullName ?? "Người dùng ẩn danh",
                    ReviewerEmail = review.Reviewer?.Email,
                    ReviewedUserId = review.ReviewedUserId,
                    ReviewedUserName = review.ReviewedUser?.FullName ?? string.Empty,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedDate = review.CreatedDate,
                    ProductType = review.Order?.VehicleOrder != null ? "Vehicle" :
                        review.Order?.BatteryOrder != null ? "Battery" : null,
                    ProductName = review.Order?.VehicleOrder != null
                        ? $"{review.Order.VehicleOrder.Vehicle?.Brand} {review.Order.VehicleOrder.Vehicle?.Model}"
                        : review.Order?.BatteryOrder != null
                            ? $"{review.Order.BatteryOrder.Battery?.Brand} {review.Order.BatteryOrder.Battery?.BatteryType}"
                            : null,
                    ProductId = review.Order?.VehicleOrder?.VehicleId ?? review.Order?.BatteryOrder?.BatteryId
                }).ToList();
            }
            catch
            {
                // Trả về empty list nếu có lỗi thay vì throw exception
                return new List<ReviewDto>();
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

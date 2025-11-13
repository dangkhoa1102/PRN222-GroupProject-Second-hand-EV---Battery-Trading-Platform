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
        private readonly IBuyerRepository _repo;
        public BuyerService(IBuyerRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> CreateOrder(CreateOrderDto order)
        {
            try
            {
                if(order == null)
                {
                    throw new Exception("Order is null");
                }
                var orderEntity = new Order
                {
                    BuyerId = order.BuyerId,
                    SellerId = order.SellerId,
                    TotalAmount = order.TotalAmount,
                    PaymentMethod = order.PaymentMethod,
                    CreatedDate = DateTime.Now,
                };
                var result = await _repo.CreateOrder(orderEntity);
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
                    ReviewedUserId = review.ReviewedUserId,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedDate = DateTime.Now,
                };
                var result = await _repo.CreateReview(reviewEntity);
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
                var result = await _repo.GetReview(reviewedUser);
                if (result == null)
                {
                    throw new Exception("No reviews found");
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<Battery>> SearchBattery(string keyword)
        {
            try
            {
                var result = await _repo.SearchBattery(keyword);
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
                var result = await _repo.SearchVehicle(keyword);
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

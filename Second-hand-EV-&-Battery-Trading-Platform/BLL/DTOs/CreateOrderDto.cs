using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
    public class CreateOrderDto
    {
        public int BuyerId { get; set; }
        public int SellerId { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
        
        // Item information
        public string ItemType { get; set; } = string.Empty; // "Vehicle" or "Battery"
        public int ItemId { get; set; } // VehicleId or BatteryId
    }
}

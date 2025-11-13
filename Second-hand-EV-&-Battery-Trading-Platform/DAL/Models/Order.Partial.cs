// Manually added to support order status tracking
#nullable disable
using System;

namespace DAL.Models;

public partial class Order
{
    // Order Status: Pending, Confirmed, Delivered, Completed, Cancelled
    public string OrderStatus { get; set; } = "Pending";
    
    // Delivery date when buyer confirms receipt
    public DateTime? DeliveryDate { get; set; }
    
    // Cancellation reason
    public string CancellationReason { get; set; }
    
    // When seller confirmed the order (reuse CompletedDate for seller confirmation)
    // CompletedDate = when seller confirmed
    // DeliveryDate = when buyer confirmed delivery
    // OrderStatus tracks the overall status
}


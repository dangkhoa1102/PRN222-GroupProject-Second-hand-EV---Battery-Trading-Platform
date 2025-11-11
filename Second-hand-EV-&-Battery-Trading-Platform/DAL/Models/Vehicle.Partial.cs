using System.Collections.Generic;

namespace DAL.Models;

public partial class Vehicle
{
    public virtual ICollection<VehicleOrder> VehicleOrders { get; set; } = new List<VehicleOrder>();
}


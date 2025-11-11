using System.Collections.Generic;

namespace DAL.Models;

public partial class Battery
{
    public virtual ICollection<BatteryOrder> BatteryOrders { get; set; } = new List<BatteryOrder>();
}


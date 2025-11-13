// Manually added to configure Order entity with new fields
#nullable disable
using Microsoft.EntityFrameworkCore;
using DAL.Models;

namespace DAL.Models;

public partial class EVTradingPlatformContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // Configure Order entity with new fields
        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(e => e.OrderStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            
            entity.Property(e => e.DeliveryDate)
                .HasColumnType("datetime");
            
            entity.Property(e => e.CancellationReason)
                .HasMaxLength(500);
        });
    }
}


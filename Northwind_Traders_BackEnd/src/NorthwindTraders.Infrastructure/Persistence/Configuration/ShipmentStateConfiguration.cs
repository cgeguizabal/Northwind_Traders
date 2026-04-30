using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthwindTraders.Domain.Entities;

namespace NorthwindTraders.Infrastructure.Persistence.Configurations;

public class ShipmentStateConfiguration : IEntityTypeConfiguration<ShipmentState>
{
    public void Configure(EntityTypeBuilder<ShipmentState> builder)
    {
        // This is our NEW table — doesn't exist in Northwind yet
        builder.ToTable("ShipmentStates");

        builder.HasKey(s => s.ShipmentStateId);

        builder.Property(s => s.Name)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(s => s.Description)
               .HasMaxLength(200);

        // ── SEED DATA ─────────────────────────────────────
        // These are the fixed rows this table will always have
        // EF Core migrations will INSERT these automatically
        // The IDs match our ShipmentStatus enum values exactly
        builder.HasData(
            new ShipmentState { ShipmentStateId = 1, Name = "Pending",    Description = "Order received, not yet processed" },
            new ShipmentState { ShipmentStateId = 2, Name = "Processing", Description = "Order is being prepared" },
            new ShipmentState { ShipmentStateId = 3, Name = "Shipped",    Description = "Order has been shipped" },
            new ShipmentState { ShipmentStateId = 4, Name = "Invoiced",   Description = "Invoice has been generated" },
            new ShipmentState { ShipmentStateId = 5, Name = "Completed",  Description = "Order delivered and completed" },
            new ShipmentState { ShipmentStateId = 6, Name = "Cancelled",  Description = "Order has been cancelled" }
        );
    }
}
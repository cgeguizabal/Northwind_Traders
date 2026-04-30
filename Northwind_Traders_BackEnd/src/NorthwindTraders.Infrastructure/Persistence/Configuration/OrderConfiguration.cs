using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthwindTraders.Domain.Entities;

namespace NorthwindTraders.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.OrderId);
        builder.Property(o => o.OrderId)
               .HasColumnName("OrderID");

        // ── EXISTING COLUMNS ──────────────────────────────
        builder.Property(o => o.Freight)
               .HasColumnType("money");             // maps to SQL money type

        builder.Property(o => o.ShipName)
               .HasMaxLength(40);

        builder.Property(o => o.ShipAddress)
               .HasMaxLength(60);

        builder.Property(o => o.ShipCity)
               .HasMaxLength(15);

        builder.Property(o => o.ShipRegion)
               .HasMaxLength(15);

        builder.Property(o => o.ShipPostalCode)
               .HasMaxLength(10);

        builder.Property(o => o.ShipCountry)
               .HasMaxLength(15);

        // ── NEW ADDRESS VALIDATION COLUMNS ────────────────
        builder.Property(o => o.OriginalShipAddress)
               .HasMaxLength(500);

        builder.Property(o => o.ValidatedShipAddress)
               .HasMaxLength(500);

        builder.Property(o => o.ShipLatitude)
               .HasColumnType("decimal(9,6)");      // precision for coordinates

        builder.Property(o => o.ShipLongitude)
               .HasColumnType("decimal(9,6)");

        // ── NEW BILLING ADDRESS COLUMNS ───────────────────
        builder.Property(o => o.BillAddress)
               .HasMaxLength(60);

        builder.Property(o => o.BillCity)
               .HasMaxLength(15);

        builder.Property(o => o.BillRegion)
               .HasMaxLength(15);

        builder.Property(o => o.BillPostalCode)
               .HasMaxLength(10);

        builder.Property(o => o.BillCountry)
               .HasMaxLength(15);

        builder.Property(o => o.OriginalBillAddress)
               .HasMaxLength(500);

        builder.Property(o => o.ValidatedBillAddress)
               .HasMaxLength(500);

        builder.Property(o => o.BillLatitude)
               .HasColumnType("decimal(9,6)");

        builder.Property(o => o.BillLongitude)
               .HasColumnType("decimal(9,6)");

        builder.Property(o => o.Notes)
               .HasMaxLength(1000);

        // ── RELATIONSHIPS ─────────────────────────────────

        // Order → Customer (many orders belong to one customer)
        builder.HasOne(o => o.Customer)
               .WithMany(c => c.Orders)
               .HasForeignKey(o => o.CustomerId)
               .HasConstraintName("FK_Orders_Customers")
               .IsRequired(false);

        // Order → Employee (many orders processed by one employee)
        builder.HasOne(o => o.Employee)
               .WithMany(e => e.Orders)
               .HasForeignKey(o => o.EmployeeId)
               .HasConstraintName("FK_Orders_Employees")
               .IsRequired(false);

        // Order → Shipper (many orders shipped by one shipper)
        builder.HasOne(o => o.Shipper)
               .WithMany(s => s.Orders)
               .HasForeignKey(o => o.ShipVia)
               .HasConstraintName("FK_Orders_Shippers")
               .IsRequired(false);

        // Order → ShipmentState (our new FK)
        builder.HasOne(o => o.ShipmentState)
               .WithMany(ss => ss.Orders)
               .HasForeignKey(o => o.ShipmentStateId)
               .IsRequired(false);
    }
}
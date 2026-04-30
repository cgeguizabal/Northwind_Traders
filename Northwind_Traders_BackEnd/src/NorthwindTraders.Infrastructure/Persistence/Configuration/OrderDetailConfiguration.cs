using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthwindTraders.Domain.Entities;

namespace NorthwindTraders.Infrastructure.Persistence.Configurations;

public class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetail>
{
    public void Configure(EntityTypeBuilder<OrderDetail> builder)
    {
        // REAL table name has a space — EF Core handles it
        builder.ToTable("Order Details");

        // COMPOSITE PRIMARY KEY — both columns together = unique
        builder.HasKey(od => new { od.OrderId, od.ProductId });

        builder.Property(od => od.OrderId)
               .HasColumnName("OrderID");

        builder.Property(od => od.ProductId)
               .HasColumnName("ProductID");

        builder.Property(od => od.UnitPrice)
               .HasColumnType("money");

        builder.Property(od => od.Discount)
               .HasColumnType("real");

        // ── RELATIONSHIPS ─────────────────────────────────
        builder.HasOne(od => od.Order)
               .WithMany(o => o.OrderDetails)
               .HasForeignKey(od => od.OrderId);

        builder.HasOne(od => od.Product)
               .WithMany(p => p.OrderDetails)
               .HasForeignKey(od => od.ProductId);
    }
}
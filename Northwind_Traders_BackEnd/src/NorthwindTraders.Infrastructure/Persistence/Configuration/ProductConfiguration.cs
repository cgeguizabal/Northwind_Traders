using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthwindTraders.Domain.Entities;

namespace NorthwindTraders.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.ProductId);
        builder.Property(p => p.ProductId)
               .HasColumnName("ProductID");

        builder.Property(p => p.ProductName)
               .IsRequired()
               .HasMaxLength(40);

        builder.Property(p => p.QuantityPerUnit)
               .HasMaxLength(20);

        builder.Property(p => p.UnitPrice)
               .HasColumnType("money");

        builder.Property(p => p.SupplierId)
               .HasColumnName("SupplierID");

        builder.Property(p => p.CategoryId)
               .HasColumnName("CategoryID");

        // ── RELATIONSHIPS ─────────────────────────────────
        builder.HasOne(p => p.Category)
               .WithMany(c => c.Products)
               .HasForeignKey(p => p.CategoryId);

        builder.HasOne(p => p.Supplier)
               .WithMany(s => s.Products)
               .HasForeignKey(p => p.SupplierId);
    }
}
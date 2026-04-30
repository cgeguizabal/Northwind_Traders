using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthwindTraders.Domain.Entities;

namespace NorthwindTraders.Infrastructure.Persistence.Configurations;

public class ShipperConfiguration : IEntityTypeConfiguration<Shipper>
{
    public void Configure(EntityTypeBuilder<Shipper> builder)
    {
        builder.ToTable("Shippers");

        builder.HasKey(s => s.ShipperId);
        builder.Property(s => s.ShipperId)
               .HasColumnName("ShipperID");

        builder.Property(s => s.CompanyName)
               .IsRequired()
               .HasMaxLength(40);

        builder.Property(s => s.Phone)
               .HasMaxLength(24);
    }
}
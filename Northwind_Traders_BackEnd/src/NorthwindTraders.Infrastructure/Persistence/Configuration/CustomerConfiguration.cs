using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthwindTraders.Domain.Entities;

namespace NorthwindTraders.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        // String primary key — nchar(5)
        builder.HasKey(c => c.CustomerId);
        builder.Property(c => c.CustomerId)
               .HasColumnName("CustomerID")
               .HasMaxLength(5)
               .IsFixedLength();               // maps to nchar (fixed length)

        builder.Property(c => c.CompanyName)
               .IsRequired()
               .HasMaxLength(40);

        builder.Property(c => c.ContactName).HasMaxLength(30);
        builder.Property(c => c.ContactTitle).HasMaxLength(30);
        builder.Property(c => c.Address).HasMaxLength(60);
        builder.Property(c => c.City).HasMaxLength(15);
        builder.Property(c => c.Region).HasMaxLength(15);
        builder.Property(c => c.PostalCode).HasMaxLength(10);
        builder.Property(c => c.Country).HasMaxLength(15);
        builder.Property(c => c.Phone).HasMaxLength(24);
        builder.Property(c => c.Fax).HasMaxLength(24);
    }
}
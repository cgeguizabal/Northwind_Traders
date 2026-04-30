using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthwindTraders.Domain.Entities;

namespace NorthwindTraders.Infrastructure.Persistence.Configurations;

public class CustomerCustomerDemoConfiguration : IEntityTypeConfiguration<CustomerCustomerDemo>
{
    public void Configure(EntityTypeBuilder<CustomerCustomerDemo> builder)
    {
        builder.ToTable("CustomerCustomerDemo");

        // Composite PK
        builder.HasKey(ccd => new { ccd.CustomerId, ccd.CustomerTypeId });

        builder.Property(ccd => ccd.CustomerId)
               .HasColumnName("CustomerID")
               .HasMaxLength(5)
               .IsFixedLength();

        builder.Property(ccd => ccd.CustomerTypeId)
               .HasColumnName("CustomerTypeID")
               .HasMaxLength(10)
               .IsFixedLength();

        builder.HasOne(ccd => ccd.Customer)
               .WithMany(c => c.CustomerCustomerDemos)
               .HasForeignKey(ccd => ccd.CustomerId);

        builder.HasOne(ccd => ccd.CustomerDemographic)
               .WithMany(cd => cd.CustomerCustomerDemos)
               .HasForeignKey(ccd => ccd.CustomerTypeId);
    }
}
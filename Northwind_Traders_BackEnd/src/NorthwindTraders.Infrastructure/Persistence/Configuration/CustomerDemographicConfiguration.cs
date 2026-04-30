using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthwindTraders.Domain.Entities;

namespace NorthwindTraders.Infrastructure.Persistence.Configurations;

public class CustomerDemographicConfiguration : IEntityTypeConfiguration<CustomerDemographic>
{
    public void Configure(EntityTypeBuilder<CustomerDemographic> builder)
    {
        builder.ToTable("CustomerDemographics");

        builder.HasKey(cd => cd.CustomerTypeId);
        builder.Property(cd => cd.CustomerTypeId)
               .HasColumnName("CustomerTypeID")
               .HasMaxLength(10)
               .IsFixedLength();               // nchar(10)
    }
}
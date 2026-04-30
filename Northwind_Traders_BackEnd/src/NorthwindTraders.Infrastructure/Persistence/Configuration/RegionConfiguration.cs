using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthwindTraders.Domain.Entities;

namespace NorthwindTraders.Infrastructure.Persistence.Configurations;

public class RegionConfiguration : IEntityTypeConfiguration<Region>
{
    public void Configure(EntityTypeBuilder<Region> builder)
    {
        builder.ToTable("Region");

        builder.HasKey(r => r.RegionId);
        builder.Property(r => r.RegionId)
               .HasColumnName("RegionID");

        builder.Property(r => r.RegionDescription)
               .IsRequired()
               .HasMaxLength(50)
               .IsFixedLength();               // nchar(50)
    }
}
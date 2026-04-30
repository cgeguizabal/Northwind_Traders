using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthwindTraders.Domain.Entities;

namespace NorthwindTraders.Infrastructure.Persistence.Configurations;

public class TerritoryConfiguration : IEntityTypeConfiguration<Territory>
{
    public void Configure(EntityTypeBuilder<Territory> builder)
    {
        builder.ToTable("Territories");

        // String PK — nvarchar(20)
        builder.HasKey(t => t.TerritoryId);
        builder.Property(t => t.TerritoryId)
               .HasColumnName("TerritoryID")
               .HasMaxLength(20);

        builder.Property(t => t.TerritoryDescription)
               .IsRequired()
               .HasMaxLength(50)
               .IsFixedLength();               // nchar(50)

        builder.Property(t => t.RegionId)
               .HasColumnName("RegionID");

        // Territory → Region
        builder.HasOne(t => t.Region)
               .WithMany(r => r.Territories)
               .HasForeignKey(t => t.RegionId);
    }
}
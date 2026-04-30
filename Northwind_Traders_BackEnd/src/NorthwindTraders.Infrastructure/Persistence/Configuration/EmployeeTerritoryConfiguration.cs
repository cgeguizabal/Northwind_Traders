using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthwindTraders.Domain.Entities;

namespace NorthwindTraders.Infrastructure.Persistence.Configurations;

public class EmployeeTerritoryConfiguration : IEntityTypeConfiguration<EmployeeTerritory>
{
    public void Configure(EntityTypeBuilder<EmployeeTerritory> builder)
    {
        builder.ToTable("EmployeeTerritories");

        // Composite PK
        builder.HasKey(et => new { et.EmployeeId, et.TerritoryId });

        builder.Property(et => et.EmployeeId)
               .HasColumnName("EmployeeID");

        builder.Property(et => et.TerritoryId)
               .HasColumnName("TerritoryID")
               .HasMaxLength(20);

        builder.HasOne(et => et.Employee)
               .WithMany(e => e.EmployeeTerritories)
               .HasForeignKey(et => et.EmployeeId);

        builder.HasOne(et => et.Territory)
               .WithMany(t => t.EmployeeTerritories)
               .HasForeignKey(et => et.TerritoryId);
    }
}
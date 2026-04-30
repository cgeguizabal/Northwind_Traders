using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthwindTraders.Domain.Entities;

namespace NorthwindTraders.Infrastructure.Persistence.Configurations;

// IEntityTypeConfiguration<T> = EF Core interface for entity mapping
// Each class configures exactly ONE entity
// This is Single Responsibility in action

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        // ── TABLE ─────────────────────────────────────────
        builder.ToTable("Employees");   // real SQL Server table name

        // ── PRIMARY KEY ───────────────────────────────────
        // Tell EF Core that EmployeeId maps to the EmployeeID column
        builder.HasKey(e => e.EmployeeId);
        builder.Property(e => e.EmployeeId)
               .HasColumnName("EmployeeID");    // real DB column name

        // ── PROPERTIES ────────────────────────────────────
        // .IsRequired() = NOT NULL in the database
        // .HasMaxLength() = maps to nvarchar(n)
        // .HasColumnName() = when C# name differs from DB column name

        builder.Property(e => e.LastName)
               .IsRequired()
               .HasMaxLength(20);

        builder.Property(e => e.FirstName)
               .IsRequired()
               .HasMaxLength(10);

        builder.Property(e => e.Title)
               .HasMaxLength(30);

        builder.Property(e => e.TitleOfCourtesy)
               .HasMaxLength(25);

        builder.Property(e => e.Address)
               .HasMaxLength(60);

        builder.Property(e => e.City)
               .HasMaxLength(15);

        builder.Property(e => e.Region)
               .HasMaxLength(15);

        builder.Property(e => e.PostalCode)
               .HasMaxLength(10);

        builder.Property(e => e.Country)
               .HasMaxLength(15);

        builder.Property(e => e.HomePhone)
               .HasMaxLength(24);

        builder.Property(e => e.Extension)
               .HasMaxLength(4);

        builder.Property(e => e.PhotoPath)
               .HasMaxLength(255);

        builder.Property(e => e.Email)
               .HasMaxLength(256);

        builder.Property(e => e.PasswordHash)
               .HasMaxLength(512);

        // ── SELF-REFERENCING RELATIONSHIP ─────────────────
        // An employee reports to another employee (their manager)
        // HasOne/WithMany describes both sides of the relationship:
        //   "one Employee HAS ONE manager"
        //   "one Employee HAS MANY direct reports"
        builder.HasOne(e => e.Manager)              // navigation to manager
               .WithMany(e => e.DirectReports)      // manager has many reports
               .HasForeignKey(e => e.ReportsTo)     // FK column
               .IsRequired(false);                  // nullable — top managers have no manager
    }
}
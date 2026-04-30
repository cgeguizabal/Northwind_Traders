using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthwindTraders.Domain.Entities;

namespace NorthwindTraders.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.CategoryId);
        builder.Property(c => c.CategoryId)
               .HasColumnName("CategoryID");

        builder.Property(c => c.CategoryName)
               .IsRequired()
               .HasMaxLength(15);

        // We deliberately IGNORE the Picture column
        // It's a legacy binary image column we don't use
        builder.Ignore(c => c.Description);        // keep Description as ntext
        builder.Property(c => c.Description)
               .HasColumnType("ntext");
    }
}
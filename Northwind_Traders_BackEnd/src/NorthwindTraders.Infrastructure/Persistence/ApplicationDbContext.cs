using Microsoft.EntityFrameworkCore;
using NorthwindTraders.Domain.Entities;

namespace NorthwindTraders.Infrastructure.Persistence;

// DbContext = EF Core's representation of your database
// It manages:
//   - The database connection
//   - Tracking changes to entities
//   - Translating C# queries to SQL
//   - Saving changes back to the database

public class ApplicationDbContext : DbContext // Inherit from EF Core's DbContext base class
{
    // Constructor receives options (connection string, provider, etc.)
    // from Dependency Injection — configured in Program.cs
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) // ApplicationDbContext is configured in Program.cs to receive options from DI, which includes the connection string and database provider (e.g., SQL Server, SQLite). We pass these options to the base DbContext constructor.
    {
    }

    // ── DbSets = your tables ──────────────────────────────
    // Each DbSet<T> represents one table in the database
    // The property name becomes the default table name
    // (we override the real names in configurations below)

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Shipper> Shippers => Set<Shipper>();
    public DbSet<Region> Regions => Set<Region>();
    public DbSet<Territory> Territories => Set<Territory>();
    public DbSet<EmployeeTerritory> EmployeeTerritories => Set<EmployeeTerritory>();
    public DbSet<CustomerDemographic> CustomerDemographics => Set<CustomerDemographic>();
    public DbSet<CustomerCustomerDemo> CustomerCustomerDemos => Set<CustomerCustomerDemo>();
    public DbSet<ShipmentState> ShipmentStates => Set<ShipmentState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // This single line scans the Infrastructure assembly
        // and automatically applies ALL IEntityTypeConfiguration classes
        // So we never have to manually call each one here
        // This is the Open/Closed principle — add new configurations
        // without ever touching this file again
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
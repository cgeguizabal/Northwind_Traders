using Microsoft.EntityFrameworkCore;            // EF Core — UseSqlServer lives here
using NorthwindTraders.Infrastructure.Persistence;
using NorthwindTraders.Infrastructure.Repositories;
using NorthwindTraders.Domain.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ── DATABASE ──────────────────────────────────────────────────────────────────

// AddDbContext — EF Core Method
// Registers ApplicationDbContext with the DI container
// Every time something asks for ApplicationDbContext,
// DI creates one with these settings automatically
builder.Services.AddDbContext<ApplicationDbContext>(options =>

    // UseSqlServer — EF Core Method
    // Tells EF Core to use SQL Server as the database provider
    // GetConnectionString — C# Method from Microsoft.Extensions.Configuration
    // Reads "DefaultConnection" value from appsettings.json
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);



// ── API BASICS ────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

var app = builder.Build();

// ── MIDDLEWARE PIPELINE ───────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

//──────── SEEDER ───────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())  // C# built in DI — creates a temporary scope
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(); // C# DI Method
    await EmployeeSeeder.SeedAsync(context);
}
app.Run();


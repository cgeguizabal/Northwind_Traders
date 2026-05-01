using Microsoft.EntityFrameworkCore;            // EF Core — UseSqlServer lives here
using NorthwindTraders.Infrastructure.Persistence;

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

app.Run();
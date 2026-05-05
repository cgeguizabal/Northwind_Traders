using Microsoft.EntityFrameworkCore;    // EF Core
using NorthwindTraders.Infrastructure.Persistence;

namespace NorthwindTraders.Infrastructure.Persistence;

public static class EmployeeSeeder
{
    // Static method — called once at app startup from Program.cs
    // Updates the 9 existing employees with emails and hashed passwords
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Dictionary — maps EmployeeId to email
        // C# built in collection — key/value pairs
        var employeeEmails = new Dictionary<int, string>
        {
            { 1, "nancy.davolio@northwind.com" },
            { 2, "andrew.fuller@northwind.com" },
            { 3, "janet.leverling@northwind.com" },
            { 4, "margaret.peacock@northwind.com" },
            { 5, "steven.buchanan@northwind.com" },
            { 6, "michael.suyama@northwind.com" },
            { 7, "robert.king@northwind.com" },
            { 8, "laura.callahan@northwind.com" },
            { 9, "anne.dodsworth@northwind.com" }
        };

        foreach (var entry in employeeEmails)
        {
            var employee = await context.Employees   // EF Core — query
                .FirstOrDefaultAsync(e => e.EmployeeId == entry.Key);  // EF Core Method

            if (employee is null) continue;

            // Only seed if not already seeded
            if (employee.Email is not null) continue;

            employee.Email        = entry.Value;
            employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Northwind2025!");
            // Force password change on first login
            employee.MustChangePassword = true;
        }

        await context.SaveChangesAsync();   // EF Core Method
    }
}
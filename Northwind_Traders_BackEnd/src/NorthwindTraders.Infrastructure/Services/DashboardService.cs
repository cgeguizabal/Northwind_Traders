using Microsoft.EntityFrameworkCore;                     // EF Core — ToListAsync, SumAsync
using NorthwindTraders.Application.DTOs.Dashboard;
using NorthwindTraders.Infrastructure.Persistence;

namespace NorthwindTraders.Infrastructure.Services;

public class DashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        // ── TOTAL ORDERS ──────────────────────────────────────────────────────
        // CountAsync — EF Core Method — SELECT COUNT(*) FROM Orders
        var totalOrders = await _context.Orders.CountAsync();

        // ── TOTAL REVENUE ─────────────────────────────────────────────────────
        // Sum across all OrderDetails: UnitPrice * Quantity * (1 - Discount)
        // C# cast to decimal needed because Discount is float
        var totalRevenue = await _context.OrderDetails
            .SumAsync(od => od.UnitPrice * od.Quantity * (decimal)(1 - od.Discount));

        // ── ORDERS BY STATUS ──────────────────────────────────────────────────
        // GroupBy — EF Core translates to SQL GROUP BY
        var ordersByStatus = await _context.Orders
            .Where(o => o.ShipmentState != null)          // exclude orders with no status
            .GroupBy(o => o.ShipmentState!.Name)          // GROUP BY ShipmentState.Name
            .Select(g => new OrdersByStatusDto
            {
                Status = g.Key,                           // the group key = status name
                Count  = g.Count()                        // COUNT(*) per group
            })
            .ToListAsync();

        // ── TOP 5 CUSTOMERS ───────────────────────────────────────────────────
        var topCustomers = await _context.Orders
            .Where(o => o.Customer != null)
            .GroupBy(o => new { o.CustomerId, o.Customer!.CompanyName })  // GROUP BY CustomerId, CompanyName
            .Select(g => new TopCustomerDto
            {
                CustomerId  = g.Key.CustomerId!,
                CompanyName = g.Key.CompanyName,
                OrderCount  = g.Count()
            })
            .OrderByDescending(x => x.OrderCount)         // most orders first
            .Take(5)                                       // TOP 5
            .ToListAsync();

        // ── TOP 5 EMPLOYEES ───────────────────────────────────────────────────
        var topEmployees = await _context.Orders
            .Where(o => o.Employee != null)
            .GroupBy(o => new
            {
                o.Employee!.FirstName,
                o.Employee.LastName
            })
            .Select(g => new TopEmployeeDto
            {
                FullName   = g.Key.FirstName + " " + g.Key.LastName,
                OrderCount = g.Count()
            })
            .OrderByDescending(x => x.OrderCount)
            .Take(5)
            .ToListAsync();

        return new DashboardDto
        {
            TotalOrders    = totalOrders,
            TotalRevenue   = totalRevenue,
            OrdersByStatus = ordersByStatus,
            TopCustomers   = topCustomers,
            TopEmployees   = topEmployees
        };
    }
}
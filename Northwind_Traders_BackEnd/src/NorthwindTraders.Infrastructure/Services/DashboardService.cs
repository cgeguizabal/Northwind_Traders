using Microsoft.EntityFrameworkCore;                     // EF Core — ToListAsync, SumAsync
using NorthwindTraders.Application.DTOs.Dashboard;
using NorthwindTraders.Infrastructure.Persistence;
using NorthwindTraders.Application.Interfaces; 

namespace NorthwindTraders.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardDto> GetDashboardAsync(DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        try
        {
        // Normalize dateTo to end-of-day so the upper bound is inclusive
        var toInclusive = dateTo.HasValue ? dateTo.Value.Date.AddDays(1) : (DateTime?)null;

        // ── TOTAL ORDERS ──────────────────────────────────────────────────────
        // CountAsync — EF Core Method — SELECT COUNT(*) FROM Orders
        var ordersQuery = _context.Orders.AsQueryable();
        if (dateFrom.HasValue)   ordersQuery = ordersQuery.Where(o => o.OrderDate >= dateFrom.Value.Date);
        if (toInclusive.HasValue) ordersQuery = ordersQuery.Where(o => o.OrderDate < toInclusive.Value);
        var totalOrders = await ordersQuery.CountAsync();

        // ── TOTAL REVENUE ─────────────────────────────────────────────────────
        var detailsQuery = _context.OrderDetails.AsQueryable();
        if (dateFrom.HasValue)   detailsQuery = detailsQuery.Where(od => od.Order!.OrderDate >= dateFrom.Value.Date);
        if (toInclusive.HasValue) detailsQuery = detailsQuery.Where(od => od.Order!.OrderDate < toInclusive.Value);
        var totalRevenue = await detailsQuery
            .SumAsync(od => od.UnitPrice * od.Quantity * (decimal)(1 - od.Discount));
        // ── TOTAL CUSTOMERS ───────────────────────────────────────────────────────────────────
        var totalCustomers = await _context.Customers.CountAsync();

        // ── TOTAL EMPLOYEES ───────────────────────────────────────────────────────────────────
        var totalEmployees = await _context.Employees.CountAsync();
        // ── ORDERS BY STATUS ──────────────────────────────────────────────────
        var ordersByStatus = await ordersQuery
            .Where(o => o.ShipmentState != null)
            .GroupBy(o => o.ShipmentState!.Name)
            .Select(g => new OrdersByStatusDto
            {
                Status = g.Key,
                Count  = g.Count()
            })
            .ToListAsync();

        // ── TOP 5 CUSTOMERS ───────────────────────────────────────────────────
        var topCustomers = await ordersQuery
            .Where(o => o.Customer != null)
            .GroupBy(o => new { o.CustomerId, o.Customer!.CompanyName })
            .Select(g => new TopCustomerDto
            {
                CustomerId  = g.Key.CustomerId!,
                CompanyName = g.Key.CompanyName,
                OrderCount  = g.Count()
            })
            .OrderByDescending(x => x.OrderCount)
            .Take(5)
            .ToListAsync();

        // ── TOP 5 EMPLOYEES ───────────────────────────────────────────────────
        var topEmployees = await ordersQuery
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
            TotalCustomers = totalCustomers,
            TotalEmployees = totalEmployees,
            OrdersByStatus = ordersByStatus,
            TopCustomers   = topCustomers,
            TopEmployees   = topEmployees
        };
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            throw new InvalidOperationException($"Database error while loading dashboard data: {ex.InnerException?.Message ?? ex.Message}", ex);
        }
        catch (OperationCanceledException ex)
        {
            throw new InvalidOperationException($"Dashboard data query was cancelled: {ex.Message}", ex);
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException($"Invalid operation while loading dashboard data: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load dashboard data: {ex.Message}", ex);
        }
    }
}
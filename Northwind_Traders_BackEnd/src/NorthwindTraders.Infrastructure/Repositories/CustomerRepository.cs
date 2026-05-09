using Microsoft.EntityFrameworkCore;
using NorthwindTraders.Domain.Entities;
using NorthwindTraders.Domain.Interfaces;
using NorthwindTraders.Infrastructure.Persistence;

namespace NorthwindTraders.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _context;

    public CustomerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Customer>> GetAllAsync()
    {
        return await _context.Customers
            .Include(c => c.Orders.Where(o => o.IsActive == "Y"))  // only active orders
            .OrderBy(c => c.CompanyName)
            .ToListAsync();
    }

    public async Task<(IReadOnlyList<Customer> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search)
    {
        var query = _context.Customers.AsQueryable();

        // Apply optional full-text search across company name, contact name, city, and country
        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.ToLower();
            query = query.Where(c =>
                (c.CompanyName  ?? "").ToLower().Contains(q) ||
                (c.ContactName  ?? "").ToLower().Contains(q) ||
                (c.City         ?? "").ToLower().Contains(q) ||
                (c.Country      ?? "").ToLower().Contains(q));
        }

        var totalCount = await query.CountAsync();

        // Skip — moves past the previous pages; Take — limits to one page of results
        var items = await query
            .OrderBy(c => c.CompanyName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new Customer
            {
                CustomerId   = c.CustomerId,
                CompanyName  = c.CompanyName,
                ContactName  = c.ContactName,
                ContactTitle = c.ContactTitle,
                City         = c.City,
                Country      = c.Country,
                Phone        = c.Phone,
                Orders       = c.Orders.Where(o => o.IsActive == "Y").ToList()
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Customer?> GetByIdAsync(string customerId)
    {
        return await _context.Customers
            .Include(c => c.Orders.Where(o => o.IsActive == "Y"))   // only active orders
                .ThenInclude(o => o.ShipmentState)                  // load status for each order
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);
    }

    public async Task<Customer?> GetByCompanyNameAsync(string companyName)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.CompanyName == companyName);
    }

    public async Task AddAsync(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
    }

    public void Update(Customer customer)
    {
        _context.Customers.Update(customer);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Order>> GetOrdersByCustomerAsync(string customerId)
    {
        return await _context.Orders
            .Where(o => o.CustomerId == customerId && o.IsActive == "Y" && o.ShipLatitude != null)
            .Include(o => o.ShipmentState)
            .Select(o => new Order
            {
                OrderId              = o.OrderId,
                ShipName             = o.ShipName,
                ValidatedShipAddress = o.ValidatedShipAddress,
                ShipLatitude         = o.ShipLatitude,
                ShipLongitude        = o.ShipLongitude,
                ShippedDate          = o.ShippedDate,
                ShipmentState        = o.ShipmentState
            })
            .ToListAsync();
    }
}
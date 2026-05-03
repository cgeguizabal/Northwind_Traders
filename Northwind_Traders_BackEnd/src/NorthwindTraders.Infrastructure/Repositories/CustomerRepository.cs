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
            .Include(c => c.Orders) 
            .OrderBy(c => c.CompanyName)
            .ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(string customerId)
    {
        return await _context.Customers
            .Include(c => c.Orders)
                .ThenInclude(o => o.ShipmentState)
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
            .Where(o => o.CustomerId == customerId && o.ShipLatitude != null)
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
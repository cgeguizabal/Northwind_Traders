using Microsoft.EntityFrameworkCore;    // EF Core
using NorthwindTraders.Domain.Entities;
using NorthwindTraders.Domain.Interfaces;
using NorthwindTraders.Infrastructure.Persistence;

namespace NorthwindTraders.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // IRepository<T> base methods

    public async Task<Order?> GetByIdAsync(int id)
        => await _context.Orders
            .FirstOrDefaultAsync(o => o.OrderId == id);    // EF Core Method

    public async Task<IReadOnlyList<Order>> GetAllAsync()
        => await _context.Orders
            .ToListAsync();                                 // EF Core Method

    public async Task AddAsync(Order order)
        => await _context.Orders.AddAsync(order);          // EF Core Method

    public void Update(Order order)
        => _context.Orders.Update(order);                  // EF Core Method

    public void Delete(Order order)
        => _context.Orders.Remove(order);                  // EF Core Method

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();              // EF Core Method

    // IOrderRepository specific methods

    public async Task<Order?> GetOrderWithDetailsAsync(int orderId)
        => await _context.Orders
            .Include(o => o.Customer)                       // EF Core Method
            .Include(o => o.Employee)                       // EF Core Method
            .Include(o => o.Shipper)                        // EF Core Method — matches Order.Shipper
            .Include(o => o.ShipmentState)                  // EF Core Method
            .Include(o => o.OrderDetails)                   // EF Core Method
                .ThenInclude(od => od.Product)              // EF Core Method — nested include
            .FirstOrDefaultAsync(o => o.OrderId == orderId); // EF Core Method

    public async Task<IReadOnlyList<Order>> GetByCustomerAsync(string customerId)
        => await _context.Orders
            .Where(o => o.CustomerId == customerId)         // EF Core Method
            .ToListAsync();                                 // EF Core Method

    public async Task<IReadOnlyList<Order>> GetByEmployeeAsync(int employeeId)
        => await _context.Orders
            .Where(o => o.EmployeeId == employeeId)         // EF Core Method
            .ToListAsync();                                 // EF Core Method

    public async Task<IReadOnlyList<Order>> GetByShipmentStatusAsync(int shipmentStateId)
        => await _context.Orders
            .Where(o => o.ShipmentStateId == shipmentStateId) // EF Core Method
            .ToListAsync();                                   // EF Core Method

    public async Task<IReadOnlyList<Order>> GetByDateRangeAsync(DateTime from, DateTime to)
        => await _context.Orders
            .Where(o => o.OrderDate >= from && o.OrderDate <= to)  // EF Core Method
            .ToListAsync();                                        // EF Core Method
}
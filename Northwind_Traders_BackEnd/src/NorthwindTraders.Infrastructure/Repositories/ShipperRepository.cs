using Microsoft.EntityFrameworkCore;
using NorthwindTraders.Domain.Entities;
using NorthwindTraders.Domain.Interfaces;
using NorthwindTraders.Infrastructure.Persistence;

namespace NorthwindTraders.Infrastructure.Repositories;

public class ShipperRepository : IShipperRepository
{
    private readonly ApplicationDbContext _context;

    public ShipperRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Shipper>> GetAllAsync()
    {
        return await _context.Shippers
            .Include(s => s.Orders)
            .OrderBy(s => s.CompanyName)
            .ToListAsync();
    }

    public async Task<Shipper?> GetByIdAsync(int id)
    {
        return await _context.Shippers
            .Include(s => s.Orders)
                .ThenInclude(o => o.Customer)
            .Include(s => s.Orders)
                .ThenInclude(o => o.ShipmentState)
            .FirstOrDefaultAsync(s => s.ShipperId == id);
    }

    // Read-only — required by IRepository<T> but never used
    public async Task AddAsync(Shipper shipper)
        => await _context.Shippers.AddAsync(shipper);

    public void Update(Shipper shipper)
        => _context.Shippers.Update(shipper);

    public void Delete(Shipper shipper)
        => _context.Shippers.Remove(shipper);

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
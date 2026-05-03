using Microsoft.EntityFrameworkCore;
using NorthwindTraders.Domain.Entities;
using NorthwindTraders.Domain.Interfaces;
using NorthwindTraders.Infrastructure.Persistence;

namespace NorthwindTraders.Infrastructure.Repositories;

public class ShipmentStateRepository : IShipmentStateRepository
{
    private readonly ApplicationDbContext _context;

    public ShipmentStateRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ShipmentState>> GetAllAsync()
    => await _context.ShipmentStates.OrderBy(s => s.ShipmentStateId).ToListAsync();

public async Task<ShipmentState?> GetByIdAsync(int id)
    => await _context.ShipmentStates.FirstOrDefaultAsync(s => s.ShipmentStateId == id);
}
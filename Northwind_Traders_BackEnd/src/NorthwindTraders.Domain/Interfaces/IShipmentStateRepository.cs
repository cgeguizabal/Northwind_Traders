using NorthwindTraders.Domain.Entities;

namespace NorthwindTraders.Domain.Interfaces;

// ShipmentStates is a simple lookup table
// Seeded once, read many times
// No write operations needed from the app

public interface IShipmentStateRepository
{
    Task<ShipmentState?> GetByIdAsync(int id);
    Task<IReadOnlyList<ShipmentState>> GetAllAsync();
}
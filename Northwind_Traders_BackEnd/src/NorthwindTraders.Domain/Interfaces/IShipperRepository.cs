using NorthwindTraders.Domain.Entities;

namespace NorthwindTraders.Domain.Interfaces;

// Shippers are a read-only lookup — the app never creates, updates, or deletes them.
// ISP: extending IReadOnlyRepository instead of IRepository avoids exposing
// Add/Update/Delete methods that have no valid implementation here.
public interface IShipperRepository : IReadOnlyRepository<Shipper>
{
}
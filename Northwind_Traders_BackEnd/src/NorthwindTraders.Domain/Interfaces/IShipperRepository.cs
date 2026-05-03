using NorthwindTraders.Domain.Entities;

namespace NorthwindTraders.Domain.Interfaces;

public interface IShipperRepository : IRepository<Shipper>
{
    // IRepository<T> covers everything — no extra methods needed
}
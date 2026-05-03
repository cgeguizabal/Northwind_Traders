using NorthwindTraders.Domain.Entities;

namespace NorthwindTraders.Domain.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    // Categories are mostly read-only — IRepository<T> covers everything we need
    // No extra methods needed
}
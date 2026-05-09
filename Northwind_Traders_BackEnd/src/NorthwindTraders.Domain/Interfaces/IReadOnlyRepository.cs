namespace NorthwindTraders.Domain.Interfaces;

// SOLID: Interface Segregation Principle
// Read-only resources (Categories, Shippers) should not be forced to implement
// Add, Update, Delete methods they will never use.
// This interface exposes only the read side of the contract.
// IRepository<T> extends this by adding write operations for mutable resources.

public interface IReadOnlyRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> GetAllAsync();
}

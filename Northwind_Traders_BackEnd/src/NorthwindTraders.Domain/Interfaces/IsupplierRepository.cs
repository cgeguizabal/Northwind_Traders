using NorthwindTraders.Domain.Entities;

namespace NorthwindTraders.Domain.Interfaces;

// Suppliers are read-only in our system — employees view but never add or delete them.
public interface ISupplierRepository
{
    Task<IReadOnlyList<Supplier>> GetAllAsync();
    Task<Supplier?> GetByIdAsync(int id);
}
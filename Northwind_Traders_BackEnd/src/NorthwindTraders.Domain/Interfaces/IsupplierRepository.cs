using NorthwindTraders.Domain.Entities;

namespace NorthwindTraders.Domain.Interfaces;

public interface ISupplierRepository
{
    Task<IReadOnlyList<Supplier>> GetAllAsync();
    Task<Supplier?> GetByIdAsync(int id);
}
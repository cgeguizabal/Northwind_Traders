using NorthwindTraders.Domain.Entities;

namespace NorthwindTraders.Domain.Interfaces;

// Products are read-only in our system
// Employees can view but not edit products
// So this interface only has read operations

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<IReadOnlyList<Product>> GetAllAsync();

    // Get products by category — useful for filtering
    Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId);

    // Get only active products (Discontinued = false)
    // We don't want to show discontinued products in the order form
    Task<IReadOnlyList<Product>> GetActiveProductsAsync();
}
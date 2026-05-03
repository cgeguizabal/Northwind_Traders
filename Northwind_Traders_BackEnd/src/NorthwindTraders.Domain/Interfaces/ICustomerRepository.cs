using NorthwindTraders.Domain.Entities;

namespace NorthwindTraders.Domain.Interfaces;

// Customer PK is string (nchar 5) not int
// So we CANNOT use IRepository<T> directly
// because IRepository<T>.GetByIdAsync takes an int
//
// We define our own full contract here instead

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(string customerId);         // string PK
    Task<IReadOnlyList<Customer>> GetAllAsync();
    Task<Customer?> GetByCompanyNameAsync(string companyName);
    Task AddAsync(Customer customer);
    void Update(Customer customer);
    Task<int> SaveChangesAsync();
    Task<IReadOnlyList<Order>> GetOrdersByCustomerAsync(string customerId);
}
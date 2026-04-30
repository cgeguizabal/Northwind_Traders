using NorthwindTraders.Domain.Entities;

namespace NorthwindTraders.Domain.Interfaces;


public interface IEmployeeRepository : IRepository<Employee>
{
    // Find an employee by their email address
    // Used during LOGIN to find the user
    Task<Employee?> GetByEmailAsync(string email);

    // Check if an email is already registered
    // Used during REGISTRATION to prevent duplicates
    Task<bool> EmailExistsAsync(string email);

    // Get all employees that report to a specific manager
    // Used to list team members under Fuller or Buchanan
    Task<IReadOnlyList<Employee>> GetByManagerAsync(int managerId);
}
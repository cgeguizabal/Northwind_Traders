using NorthwindTraders.Domain.Entities;

namespace NorthwindTraders.Application.Interfaces;

// Generates signed JWT tokens used to authenticate API requests.
public interface IJwtService
{
    // Returns a signed JWT string for the given employee
    string GenerateToken(Employee employee);
}
using NorthwindTraders.Domain.Entities;

namespace NorthwindTraders.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(Employee employee);
}
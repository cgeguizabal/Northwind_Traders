using Microsoft.EntityFrameworkCore;      
using NorthwindTraders.Domain.Entities;
using NorthwindTraders.Domain.Interfaces;
using NorthwindTraders.Infrastructure.Persistence;

namespace NorthwindTraders.Infrastructure.Repositories;

public class EmployeeRepository: IEmployeeRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // ── IRepository<Employee> base methods ───────────────────

    public async Task<Employee?> GetByIdAsync(int id)
        => await _context.Employees
                         .Include(e => e.Manager)           // EF Core Method — loads Manager navigation property
                         .Include(e => e.DirectReports)     // EF Core Method — loads DirectReports navigation property
                         .FirstOrDefaultAsync(e => e.EmployeeId == id); // EF Core Method — SELECT WHERE + LIMIT 1

    public async Task<IReadOnlyList<Employee>> GetAllAsync()
        => await _context.Employees
                         .Include(e => e.Manager)           // EF Core Method
                         .ToListAsync();                     // EF Core Method — SELECT *

    public async Task AddAsync(Employee employee)
        => await _context.Employees.AddAsync(employee);     // EF Core Method — stages INSERT

    public void Update(Employee employee)
        => _context.Employees.Update(employee);             // EF Core Method — stages UPDATE

    public void Delete(Employee employee)
        => _context.Employees.Remove(employee);             // EF Core Method — stages DELETE

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();               // EF Core Method — runs all staged SQL

    // ── IEmployeeRepository specific methods ─────────────────

    public async Task<Employee?> GetByEmailAsync(string email)
        => await _context.Employees
                         .FirstOrDefaultAsync(e => e.Email == email); // EF Core Method

    public async Task<bool> EmailExistsAsync(string email)
        => await _context.Employees
                         .AnyAsync(e => e.Email == email);  // EF Core Method — SELECT EXISTS

    public async Task<IReadOnlyList<Employee>> GetByManagerAsync(int managerId)
        => await _context.Employees
                         .Where(e => e.ReportsTo == managerId) // EF Core Method — SELECT WHERE
                         .ToListAsync();                        // EF Core Method
}
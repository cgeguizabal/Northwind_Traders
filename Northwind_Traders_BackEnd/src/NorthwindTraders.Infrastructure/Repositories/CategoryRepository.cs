using Microsoft.EntityFrameworkCore;
using NorthwindTraders.Domain.Entities;
using NorthwindTraders.Domain.Interfaces;
using NorthwindTraders.Infrastructure.Persistence;

namespace NorthwindTraders.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync()
    {
        return await _context.Categories
            .Include(c => c.Products)   // load products so the controller can count them
            .OrderBy(c => c.CategoryName)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .Include(c => c.Products)   // load full product list for the detail view
            .FirstOrDefaultAsync(c => c.CategoryId == id);
    }
}
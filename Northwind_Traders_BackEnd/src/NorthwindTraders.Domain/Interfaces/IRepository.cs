namespace NorthwindTraders.Domain.Interfaces;

// SOLID: Interface Segregation — this is the BASE contract
// with operations ALL repositories share
// Specific repositories will extend this with their own methods
//
// PATTERN: Repository Pattern
// Abstracts all data access behind an interface
// Application never talks to the database directly

public interface IRepository<T> where T : class
{
   // ── READ ──────────────────────────────────────────────

    // Get a single entity by its primary key
    // Task = async operation (database calls are always async)
    // T? = might return null if not found
    Task<T?> GetByIdAsync(int id);

    // Get all entities of this type
    // IReadOnlyList = you get back a list you can read but not modify
    // This is intentional — you don't modify DB data through a list
    Task<IReadOnlyList<T>> GetAllAsync();

    // ── WRITE ─────────────────────────────────────────────

    // Add a new entity to the database
    Task AddAsync(T entity);

    // Update an existing entity
    void Update(T entity);                  // not async — EF Core tracks changes in memory

    // Remove an entity
    void Delete(T entity);                  // not async — same reason

    // ── SAVE ──────────────────────────────────────────────

    // Persist all pending changes to the database
    // This is the actual SQL INSERT/UPDATE/DELETE
    // Returns number of rows affected
    Task<int> SaveChangesAsync();
}
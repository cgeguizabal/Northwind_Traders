namespace NorthwindTraders.Domain.Interfaces;

// SOLID: Interface Segregation — this is the BASE contract
// with operations ALL repositories share
// Specific repositories will extend this with their own methods
//
// PATTERN: Repository Pattern
// Abstracts all data access behind an interface
// Application never talks to the database directly

public interface IRepository<T> : IReadOnlyRepository<T> where T : class
{
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
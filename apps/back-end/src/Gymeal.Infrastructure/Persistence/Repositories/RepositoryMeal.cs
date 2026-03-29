using Gymeal.Domain.Common;
using Gymeal.Domain.Entities;
using Gymeal.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gymeal.Infrastructure.Persistence.Repositories;

// NOTE: All DB methods return Result<T> to expose the failure path explicitly.
// Callers pattern-match on IsSuccess/IsFailure — no silent null returns.
// Reference: RULE.md §5.6 (Repository Result Pattern)
public sealed class RepositoryMeal(AppDbContext context) : IMealRepository
{
    public async Task<Result<Meal>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // N+1 prevention: eager-load Provider so callers access Provider.Name without extra query
        Meal? meal = await context.Meals
            .Include(m => m.Provider)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        return meal is null
            ? Error.NotFound("Meal", id)
            : Result<Meal>.Success(meal);
    }

    public async Task<Result<IReadOnlyList<Meal>>> GetPagedAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        // N+1 prevention: Include Provider to avoid per-row Provider queries in handler
        IReadOnlyList<Meal> meals = await context.Meals
            .Include(m => m.Provider)
            .Where(m => m.IsAvailable)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<Meal>>.Success(meals);
    }

    public async Task<Result<IReadOnlyList<Meal>>> SearchAsync(
        string query, int limit, CancellationToken cancellationToken = default)
    {
        // Delegated to ISearchService (pg_trgm via ServiceTrgmSearch) — this method
        // acts as a fallback direct EF query for simple contains-based search.
        // SECURITY: Using parameterized LINQ — no raw SQL string concatenation.
        IReadOnlyList<Meal> meals = await context.Meals
            .Include(m => m.Provider)
            .Where(m => m.IsAvailable && EF.Functions.ILike(m.Name, $"%{query}%"))
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<Meal>>.Success(meals);
    }

    public async Task<Result<IReadOnlyList<Meal>>> GetSimilarAsync(
        Guid mealId, int limit, CancellationToken cancellationToken = default)
    {
        // pgvector cosine similarity — parameterized via FormattableString (safe)
        // The subquery retrieves the source meal's embedding and finds closest vectors.
        // SECURITY: mealId is a Guid — no injection risk; FormattableString is parameterized.
        IReadOnlyList<Meal> meals = await context.Meals
            .FromSqlInterpolated($"""
                SELECT m.* FROM meals m
                JOIN meals source ON source.id = {mealId}
                WHERE m.id != {mealId}
                  AND m.deleted_at IS NULL
                  AND m.is_available = true
                  AND m.embedding IS NOT NULL
                  AND source.embedding IS NOT NULL
                ORDER BY m.embedding <=> source.embedding
                LIMIT {limit}
                """)
            .Include(m => m.Provider)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<Meal>>.Success(meals);
    }

    public async Task<Result<Meal>> AddAsync(Meal meal, CancellationToken cancellationToken = default)
    {
        await context.Meals.AddAsync(meal, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return Result<Meal>.Success(meal);
    }

    public async Task<Result<Meal>> UpdateAsync(Meal meal, CancellationToken cancellationToken = default)
    {
        context.Meals.Update(meal);
        await context.SaveChangesAsync(cancellationToken);
        return Result<Meal>.Success(meal);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        await context.Meals.CountAsync(m => m.IsAvailable, cancellationToken);
}

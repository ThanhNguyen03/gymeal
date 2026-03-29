using Gymeal.Domain.Entities;
using Gymeal.Domain.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Gymeal.Infrastructure.Persistence;

namespace Gymeal.Infrastructure.Services;

/// <summary>
/// pg_trgm trigram similarity search implementation.
/// </summary>
/// <remarks>
/// SECURITY: Uses FromSqlInterpolated which produces parameterized SQL — never string concat.
/// pg_trgm requires: CREATE EXTENSION IF NOT EXISTS pg_trgm (in migration).
/// GIN trigram indexes on meals.name and meals.description enable fast similarity queries.
/// Reference: PLAN.md §4.4
/// </remarks>
public sealed class ServiceTrgmSearch(AppDbContext context) : ISearchService
{
    private const double SimilarityThreshold = 0.15;

    public async Task<IReadOnlyList<Meal>> SearchMealsAsync(
        string query,
        int limit,
        CancellationToken cancellationToken = default)
    {
        // SECURITY: FromSqlInterpolated produces parameterized SQL — {query} is a parameter,
        // NOT string interpolation into the SQL text. Safe against SQL injection.
        // NOTE: word_similarity scores partial word matches (useful for substring queries like "chick" → "Chicken")
        IReadOnlyList<Meal> results = await context.Meals
            .FromSqlInterpolated($"""
                SELECT m.* FROM meals m
                WHERE m.deleted_at IS NULL
                  AND m.is_available = true
                  AND (
                    word_similarity({query}, m.name) > {SimilarityThreshold}
                    OR word_similarity({query}, m.description) > {SimilarityThreshold}
                    OR m.name ILIKE {$"%{query}%"}
                  )
                ORDER BY GREATEST(
                    word_similarity({query}, m.name),
                    word_similarity({query}, m.description)
                ) DESC
                LIMIT {limit}
                """)
            .Include(m => m.Provider)
            .ToListAsync(cancellationToken);

        return results;
    }
}

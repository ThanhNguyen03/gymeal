using Gymeal.Domain.Entities;
using Gymeal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Gymeal.Infrastructure.GraphQL.DataLoaders;

// PERFORMANCE: DataLoader batches N+1 queries.
// Loads all available meals for a batch of provider IDs in a single query,
// then groups them by provider ID for distribution back to resolvers.
public sealed class DataLoaderMealsByProviderId(AppDbContext context)
    : GroupedDataLoader<Guid, Meal>(new DataLoaderOptions())
{
    protected override async Task<ILookup<Guid, Meal>> LoadGroupedBatchAsync(
        IReadOnlyList<Guid> keys,
        CancellationToken cancellationToken)
    {
        List<Meal> meals = await context.Meals
            .Where(m => keys.Contains(m.ProviderId) && m.IsAvailable)
            .ToListAsync(cancellationToken);

        return meals.ToLookup(m => m.ProviderId);
    }
}

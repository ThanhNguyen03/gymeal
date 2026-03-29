using Gymeal.Domain.Entities;
using Gymeal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Gymeal.Infrastructure.GraphQL.DataLoaders;

// PERFORMANCE: DataLoader batches N+1 queries.
// Instead of 1 query per provider ID, all IDs requested in one execution are
// collected and resolved in a single IN (...) query.
public sealed class DataLoaderProviderById(AppDbContext context)
    : BatchDataLoader<Guid, Provider>(new DataLoaderOptions())
{
    protected override async Task<IReadOnlyDictionary<Guid, Provider>> LoadBatchAsync(
        IReadOnlyList<Guid> keys,
        CancellationToken cancellationToken)
    {
        return await context.Providers
            .Where(p => keys.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);
    }
}

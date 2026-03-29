using Gymeal.Domain.Entities;
using Gymeal.Infrastructure.GraphQL.DataLoaders;

namespace Gymeal.Infrastructure.GraphQL.Types;

[ObjectType<Provider>]
public static partial class TypeProvider
{
    static partial void Configure(IObjectTypeDescriptor<Provider> descriptor)
    {
        descriptor.Name("Provider");
        descriptor.Description("A meal provider (restaurant or meal prep service) in the Gymeal platform.");
    }

    // PERFORMANCE: DataLoader batches N+1 — all meal loads for a provider list
    // are resolved in a single WHERE provider_id IN (...) query.
    public static async Task<IEnumerable<Meal>> GetMealsAsync(
        [Parent] Provider provider,
        DataLoaderMealsByProviderId dataLoader,
        CancellationToken cancellationToken)
    {
        return await dataLoader.LoadAsync(provider.Id, cancellationToken);
    }
}

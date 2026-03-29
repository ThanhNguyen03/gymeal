using Gymeal.Domain.Entities;
using Gymeal.Domain.Enums;
using Gymeal.Infrastructure.GraphQL.DataLoaders;

namespace Gymeal.Infrastructure.GraphQL.Types;

[ObjectType<Meal>]
public static partial class TypeMeal
{
    static partial void Configure(IObjectTypeDescriptor<Meal> descriptor)
    {
        descriptor.Name("Meal");
        descriptor.Description("A meal item in the Gymeal catalog.");

        // SECURITY: Embedding is an internal pgvector column — never expose via API.
        // Exposing embedding vectors would allow reverse-engineering of the ML model
        // and could leak information about nutritional data patterns.
        descriptor.Ignore(m => m.Embedding);

        descriptor.Field(m => m.PriceInCents)
            .Description("Price in integer cents (e.g. 1999 = $19.99). Integer avoids floating-point precision issues.");
    }

    public static async Task<Provider> GetProviderAsync(
        [Parent] Meal meal,
        DataLoaderProviderById dataLoader,
        CancellationToken cancellationToken)
    {
        return await dataLoader.LoadAsync(meal.ProviderId, cancellationToken);
    }
}

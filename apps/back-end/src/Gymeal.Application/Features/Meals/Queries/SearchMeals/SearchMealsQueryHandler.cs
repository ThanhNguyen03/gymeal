using Gymeal.Application.Features.Meals.DTOs;
using Gymeal.Domain.Common;
using Gymeal.Domain.Entities;
using Gymeal.Domain.Interfaces.Services;
using MediatR;

namespace Gymeal.Application.Features.Meals.Queries.SearchMeals;

public sealed class SearchMealsQueryHandler(
    ISearchService searchService,
    ICacheService cacheService) : IRequestHandler<SearchMealsQuery, Result<IReadOnlyList<MealSummaryDto>>>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public async Task<Result<IReadOnlyList<MealSummaryDto>>> Handle(
        SearchMealsQuery request,
        CancellationToken cancellationToken)
    {
        // Normalize key to prevent cache poisoning via differently-cased queries
        string cacheKey = $"search:{request.Query.ToLowerInvariant().Trim()}:{request.Limit}";

        IReadOnlyList<MealSummaryDto>? cached = await cacheService.GetAsync<IReadOnlyList<MealSummaryDto>>(
            cacheKey, cancellationToken);

        if (cached is not null)
        {
            return Result<IReadOnlyList<MealSummaryDto>>.Success(cached);
        }

        IReadOnlyList<Meal> meals = await searchService.SearchMealsAsync(
            request.Query, request.Limit, cancellationToken);

        IReadOnlyList<MealSummaryDto> results = meals
            .Select(m => new MealSummaryDto(
                Id: m.Id,
                Name: m.Name,
                ImageUrl: m.ImageUrl,
                Category: m.Category,
                Calories: m.Calories,
                ProteinG: m.ProteinG,
                PriceInCents: m.PriceInCents,
                ProviderName: m.Provider.Name,
                IsAvailable: m.IsAvailable))
            .ToList()
            .AsReadOnly();

        await cacheService.SetAsync(cacheKey, results, CacheTtl, cancellationToken);

        return Result<IReadOnlyList<MealSummaryDto>>.Success(results);
    }
}

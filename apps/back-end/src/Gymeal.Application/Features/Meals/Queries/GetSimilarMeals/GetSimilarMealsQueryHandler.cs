using Gymeal.Application.Features.Meals.DTOs;
using Gymeal.Domain.Common;
using Gymeal.Domain.Entities;
using Gymeal.Domain.Interfaces.Repositories;
using MediatR;

namespace Gymeal.Application.Features.Meals.Queries.GetSimilarMeals;

public sealed class GetSimilarMealsQueryHandler(IMealRepository mealRepository)
    : IRequestHandler<GetSimilarMealsQuery, Result<IReadOnlyList<MealSummaryDto>>>
{
    public async Task<Result<IReadOnlyList<MealSummaryDto>>> Handle(
        GetSimilarMealsQuery request,
        CancellationToken cancellationToken)
    {
        // Verify the source meal exists before running similarity query
        Result<Meal> sourceResult = await mealRepository.GetByIdAsync(request.MealId, cancellationToken);
        if (sourceResult.IsFailure)
        {
            return sourceResult.Error;
        }

        Result<IReadOnlyList<Meal>> similarResult = await mealRepository.GetSimilarAsync(
            request.MealId, request.First, cancellationToken);

        if (similarResult.IsFailure)
        {
            return similarResult.Error;
        }

        IReadOnlyList<MealSummaryDto> results = similarResult.Value
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

        return Result<IReadOnlyList<MealSummaryDto>>.Success(results);
    }
}

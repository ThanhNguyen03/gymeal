using Gymeal.Application.Features.Meals.DTOs;
using Gymeal.Domain.Common;
using Gymeal.Domain.Entities;
using Gymeal.Domain.Interfaces.Repositories;
using MediatR;

namespace Gymeal.Application.Features.Meals.Queries.GetMealById;

public sealed class GetMealByIdQueryHandler(IMealRepository mealRepository)
    : IRequestHandler<GetMealByIdQuery, Result<MealDto>>
{
    public async Task<Result<MealDto>> Handle(
        GetMealByIdQuery request,
        CancellationToken cancellationToken)
    {
        Result<Meal> mealResult = await mealRepository.GetByIdAsync(request.Id, cancellationToken);
        if (mealResult.IsFailure)
        {
            return mealResult.Error;
        }

        Meal meal = mealResult.Value;

        return new MealDto(
            Id: meal.Id,
            ProviderId: meal.ProviderId,
            ProviderName: meal.Provider.Name,
            Name: meal.Name,
            Description: meal.Description,
            ImageUrl: meal.ImageUrl,
            Category: meal.Category,
            PriceInCents: meal.PriceInCents,
            Calories: meal.Calories,
            ProteinG: meal.ProteinG,
            CarbsG: meal.CarbsG,
            FatG: meal.FatG,
            FiberG: meal.FiberG,
            Ingredients: meal.Ingredients.AsReadOnly(),
            AllergenTags: meal.AllergenTags.AsReadOnly(),
            FitnessGoalTags: meal.FitnessGoalTags.AsReadOnly(),
            IsAvailable: meal.IsAvailable,
            CreatedAt: meal.CreatedAt,
            UpdatedAt: meal.UpdatedAt);
    }
}

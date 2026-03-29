using Gymeal.Application.Features.Meals.DTOs;
using Gymeal.Domain.Common;
using Gymeal.Domain.Entities;
using Gymeal.Domain.Interfaces.Repositories;
using Gymeal.Domain.Interfaces.Services;
using MediatR;

namespace Gymeal.Application.Features.Meals.Commands.UpdateMeal;

public sealed class UpdateMealCommandHandler(
    IMealRepository mealRepository,
    IProviderRepository providerRepository,
    ICacheService cacheService,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTime) : IRequestHandler<UpdateMealCommand, Result<MealDto>>
{
    public async Task<Result<MealDto>> Handle(
        UpdateMealCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            return Error.Unauthorized();
        }

        Result<Provider> providerResult = await providerRepository.GetByUserIdAsync(
            currentUser.UserId.Value, cancellationToken);

        if (providerResult.IsFailure)
        {
            return Error.Forbidden("You must have a provider account to update meals.");
        }

        Provider provider = providerResult.Value;

        Result<Meal> mealResult = await mealRepository.GetByIdAsync(request.MealId, cancellationToken);
        if (mealResult.IsFailure)
        {
            return mealResult.Error;
        }

        Meal meal = mealResult.Value;

        // SECURITY: Verify caller owns this meal
        if (meal.ProviderId != provider.Id)
        {
            return Error.Forbidden("You do not own this meal.");
        }

        // Apply only provided fields (partial update)
        if (request.Name is not null) meal.Name = request.Name;
        if (request.Description is not null) meal.Description = request.Description;
        if (request.ImageUrl is not null) meal.ImageUrl = request.ImageUrl;
        if (request.Category.HasValue) meal.Category = request.Category.Value;
        if (request.PriceInCents.HasValue) meal.PriceInCents = request.PriceInCents.Value;
        if (request.Calories.HasValue) meal.Calories = request.Calories.Value;
        if (request.ProteinG.HasValue) meal.ProteinG = request.ProteinG.Value;
        if (request.CarbsG.HasValue) meal.CarbsG = request.CarbsG.Value;
        if (request.FatG.HasValue) meal.FatG = request.FatG.Value;
        if (request.FiberG.HasValue) meal.FiberG = request.FiberG.Value;
        if (request.Ingredients is not null) meal.Ingredients = request.Ingredients;
        if (request.AllergenTags is not null) meal.AllergenTags = request.AllergenTags;
        if (request.FitnessGoalTags is not null) meal.FitnessGoalTags = request.FitnessGoalTags;

        meal.UpdatedAt = dateTime.UtcNow;

        Result<Meal> updateResult = await mealRepository.UpdateAsync(meal, cancellationToken);
        if (updateResult.IsFailure)
        {
            return updateResult.Error;
        }

        // Invalidate search cache after mutation
        await cacheService.RemoveByPrefixAsync("search:", cancellationToken);

        Meal saved = updateResult.Value;

        return new MealDto(
            Id: saved.Id,
            ProviderId: saved.ProviderId,
            ProviderName: provider.Name,
            Name: saved.Name,
            Description: saved.Description,
            ImageUrl: saved.ImageUrl,
            Category: saved.Category,
            PriceInCents: saved.PriceInCents,
            Calories: saved.Calories,
            ProteinG: saved.ProteinG,
            CarbsG: saved.CarbsG,
            FatG: saved.FatG,
            FiberG: saved.FiberG,
            Ingredients: saved.Ingredients.AsReadOnly(),
            AllergenTags: saved.AllergenTags.AsReadOnly(),
            FitnessGoalTags: saved.FitnessGoalTags.AsReadOnly(),
            IsAvailable: saved.IsAvailable,
            CreatedAt: saved.CreatedAt,
            UpdatedAt: saved.UpdatedAt);
    }
}

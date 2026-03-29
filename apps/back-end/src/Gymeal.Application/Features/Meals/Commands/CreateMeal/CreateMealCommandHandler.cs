using Gymeal.Application.Features.Meals.DTOs;
using Gymeal.Domain.Common;
using Gymeal.Domain.Entities;
using Gymeal.Domain.Interfaces.Repositories;
using Gymeal.Domain.Interfaces.Services;
using MediatR;

namespace Gymeal.Application.Features.Meals.Commands.CreateMeal;

public sealed class CreateMealCommandHandler(
    IMealRepository mealRepository,
    IProviderRepository providerRepository,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTime) : IRequestHandler<CreateMealCommand, Result<MealDto>>
{
    public async Task<Result<MealDto>> Handle(
        CreateMealCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            return Error.Unauthorized();
        }

        // Verify caller owns a provider account
        Result<Provider> providerResult = await providerRepository.GetByUserIdAsync(
            currentUser.UserId.Value, cancellationToken);

        if (providerResult.IsFailure)
        {
            return Error.Forbidden("You must have a provider account to create meals.");
        }

        Provider provider = providerResult.Value;

        Meal meal = new()
        {
            ProviderId = provider.Id,
            Name = request.Name,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            Category = request.Category,
            PriceInCents = request.PriceInCents,
            Calories = request.Calories,
            ProteinG = request.ProteinG,
            CarbsG = request.CarbsG,
            FatG = request.FatG,
            FiberG = request.FiberG,
            Ingredients = request.Ingredients,
            AllergenTags = request.AllergenTags,
            FitnessGoalTags = request.FitnessGoalTags,
            IsAvailable = true,
            CreatedAt = dateTime.UtcNow,
            UpdatedAt = dateTime.UtcNow,
        };

        Result<Meal> addResult = await mealRepository.AddAsync(meal, cancellationToken);
        if (addResult.IsFailure)
        {
            return addResult.Error;
        }

        Meal saved = addResult.Value;

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

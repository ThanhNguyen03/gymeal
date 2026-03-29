using Gymeal.Domain.Common;
using Gymeal.Domain.Entities;
using Gymeal.Domain.Interfaces.Repositories;
using Gymeal.Domain.Interfaces.Services;
using MediatR;

namespace Gymeal.Application.Features.Meals.Commands.ToggleMealAvailability;

public sealed class ToggleMealAvailabilityCommandHandler(
    IMealRepository mealRepository,
    IProviderRepository providerRepository,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTime) : IRequestHandler<ToggleMealAvailabilityCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        ToggleMealAvailabilityCommand request,
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
            return Error.Forbidden("You must have a provider account to manage meals.");
        }

        Result<Meal> mealResult = await mealRepository.GetByIdAsync(request.MealId, cancellationToken);
        if (mealResult.IsFailure)
        {
            return mealResult.Error;
        }

        Meal meal = mealResult.Value;

        // SECURITY: Verify caller owns this meal
        if (meal.ProviderId != providerResult.Value.Id)
        {
            return Error.Forbidden("You do not own this meal.");
        }

        meal.IsAvailable = !meal.IsAvailable;
        meal.UpdatedAt = dateTime.UtcNow;

        Result<Meal> updateResult = await mealRepository.UpdateAsync(meal, cancellationToken);
        if (updateResult.IsFailure)
        {
            return updateResult.Error;
        }

        return Result<bool>.Success(meal.IsAvailable);
    }
}

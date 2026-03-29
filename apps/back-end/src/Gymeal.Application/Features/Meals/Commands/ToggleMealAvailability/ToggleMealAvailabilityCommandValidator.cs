using FluentValidation;

namespace Gymeal.Application.Features.Meals.Commands.ToggleMealAvailability;

public sealed class ToggleMealAvailabilityCommandValidator : AbstractValidator<ToggleMealAvailabilityCommand>
{
    public ToggleMealAvailabilityCommandValidator()
    {
        RuleFor(x => x.MealId).NotEmpty();
    }
}

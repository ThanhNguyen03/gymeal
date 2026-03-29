using FluentValidation;

namespace Gymeal.Application.Features.Meals.Commands.CreateMeal;

public sealed class CreateMealCommandValidator : AbstractValidator<CreateMealCommand>
{
    public CreateMealCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.PriceInCents).GreaterThan(0);
        RuleFor(x => x.Calories).GreaterThan(0);
        RuleFor(x => x.ProteinG).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CarbsG).GreaterThanOrEqualTo(0);
        RuleFor(x => x.FatG).GreaterThanOrEqualTo(0);
        RuleFor(x => x.FiberG).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Ingredients).NotNull();
        RuleFor(x => x.AllergenTags).NotNull();
        RuleFor(x => x.FitnessGoalTags).NotNull();
    }
}

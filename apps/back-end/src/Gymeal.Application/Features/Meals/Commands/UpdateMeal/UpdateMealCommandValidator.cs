using FluentValidation;

namespace Gymeal.Application.Features.Meals.Commands.UpdateMeal;

public sealed class UpdateMealCommandValidator : AbstractValidator<UpdateMealCommand>
{
    public UpdateMealCommandValidator()
    {
        RuleFor(x => x.MealId).NotEmpty();
        RuleFor(x => x.Name).MaximumLength(200).When(x => x.Name is not null);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
        RuleFor(x => x.PriceInCents).GreaterThan(0).When(x => x.PriceInCents.HasValue);
        RuleFor(x => x.Calories).GreaterThan(0).When(x => x.Calories.HasValue);
        RuleFor(x => x.ProteinG).GreaterThanOrEqualTo(0).When(x => x.ProteinG.HasValue);
        RuleFor(x => x.CarbsG).GreaterThanOrEqualTo(0).When(x => x.CarbsG.HasValue);
        RuleFor(x => x.FatG).GreaterThanOrEqualTo(0).When(x => x.FatG.HasValue);
        RuleFor(x => x.FiberG).GreaterThanOrEqualTo(0).When(x => x.FiberG.HasValue);
    }
}

using FluentValidation;

namespace Gymeal.Application.Features.Meals.Queries.GetSimilarMeals;

public sealed class GetSimilarMealsQueryValidator : AbstractValidator<GetSimilarMealsQuery>
{
    public GetSimilarMealsQueryValidator()
    {
        RuleFor(x => x.MealId).NotEmpty();
        RuleFor(x => x.First).InclusiveBetween(1, 20);
    }
}

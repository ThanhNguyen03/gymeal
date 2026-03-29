using FluentValidation;

namespace Gymeal.Application.Features.Meals.Queries.GetMeals;

public sealed class GetMealsQueryValidator : AbstractValidator<GetMealsQuery>
{
    public GetMealsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}

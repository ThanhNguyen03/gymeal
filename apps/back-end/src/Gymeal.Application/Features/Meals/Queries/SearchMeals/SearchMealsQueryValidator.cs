using FluentValidation;

namespace Gymeal.Application.Features.Meals.Queries.SearchMeals;

public sealed class SearchMealsQueryValidator : AbstractValidator<SearchMealsQuery>
{
    public SearchMealsQueryValidator()
    {
        RuleFor(x => x.Query).MinimumLength(2).MaximumLength(100);
        RuleFor(x => x.Limit).InclusiveBetween(1, 50);
    }
}

using FluentValidation;

namespace Gymeal.Application.Features.Meals.Queries.GetMealById;

public sealed class GetMealByIdQueryValidator : AbstractValidator<GetMealByIdQuery>
{
    public GetMealByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

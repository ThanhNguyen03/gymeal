using FluentValidation;

namespace Gymeal.Application.Features.Providers.Queries.GetProviders;

public sealed class GetProvidersQueryValidator : AbstractValidator<GetProvidersQuery>
{
    public GetProvidersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}

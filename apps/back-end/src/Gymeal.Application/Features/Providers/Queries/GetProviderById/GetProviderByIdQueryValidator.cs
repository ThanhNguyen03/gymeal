using FluentValidation;

namespace Gymeal.Application.Features.Providers.Queries.GetProviderById;

public sealed class GetProviderByIdQueryValidator : AbstractValidator<GetProviderByIdQuery>
{
    public GetProviderByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

using FluentValidation;

namespace Gymeal.Application.Features.Providers.Commands.VerifyProvider;

public sealed class VerifyProviderCommandValidator : AbstractValidator<VerifyProviderCommand>
{
    public VerifyProviderCommandValidator()
    {
        RuleFor(x => x.ProviderId).NotEmpty();
    }
}

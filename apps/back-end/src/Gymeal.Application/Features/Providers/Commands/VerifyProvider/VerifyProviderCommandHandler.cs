using Gymeal.Application.Features.Providers.DTOs;
using Gymeal.Domain.Common;
using Gymeal.Domain.Entities;
using Gymeal.Domain.Interfaces.Repositories;
using Gymeal.Domain.Interfaces.Services;
using MediatR;

namespace Gymeal.Application.Features.Providers.Commands.VerifyProvider;

public sealed class VerifyProviderCommandHandler(
    IProviderRepository providerRepository,
    ICurrentUserService currentUser) : IRequestHandler<VerifyProviderCommand, Result<ProviderDto>>
{
    public async Task<Result<ProviderDto>> Handle(
        VerifyProviderCommand request,
        CancellationToken cancellationToken)
    {
        // Defense-in-depth: re-validate Admin role in handler even though controller
        // already enforces [Authorize(Roles = "Admin")]
        if (currentUser.UserId is null || currentUser.Role != "Admin")
        {
            return Error.Forbidden("Only admins can verify providers.");
        }

        Result<Provider> verifyResult = await providerRepository.VerifyAsync(
            request.ProviderId, cancellationToken);

        if (verifyResult.IsFailure)
        {
            return verifyResult.Error;
        }

        Provider provider = verifyResult.Value;

        return new ProviderDto(
            Id: provider.Id,
            UserId: provider.UserId,
            Name: provider.Name,
            Description: provider.Description,
            LogoUrl: provider.LogoUrl,
            CuisineTags: provider.CuisineTags.AsReadOnly(),
            IsVerified: provider.IsVerified,
            Rating: provider.Rating,
            TotalOrders: provider.TotalOrders,
            CreatedAt: provider.CreatedAt);
    }
}

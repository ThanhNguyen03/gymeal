using Gymeal.Application.Features.Providers.DTOs;
using Gymeal.Domain.Common;
using Gymeal.Domain.Entities;
using Gymeal.Domain.Interfaces.Repositories;
using MediatR;

namespace Gymeal.Application.Features.Providers.Queries.GetProviderById;

public sealed class GetProviderByIdQueryHandler(IProviderRepository providerRepository)
    : IRequestHandler<GetProviderByIdQuery, Result<ProviderDto>>
{
    public async Task<Result<ProviderDto>> Handle(
        GetProviderByIdQuery request,
        CancellationToken cancellationToken)
    {
        Result<Provider> providerResult = await providerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (providerResult.IsFailure)
        {
            return providerResult.Error;
        }

        Provider provider = providerResult.Value;

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

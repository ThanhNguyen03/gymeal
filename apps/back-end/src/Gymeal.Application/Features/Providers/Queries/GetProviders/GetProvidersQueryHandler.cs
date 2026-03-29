using Gymeal.Application.Common.Pagination;
using Gymeal.Application.Features.Providers.DTOs;
using Gymeal.Domain.Common;
using Gymeal.Domain.Entities;
using Gymeal.Domain.Interfaces.Repositories;
using MediatR;

namespace Gymeal.Application.Features.Providers.Queries.GetProviders;

public sealed class GetProvidersQueryHandler(IProviderRepository providerRepository)
    : IRequestHandler<GetProvidersQuery, Result<PagedResult<ProviderSummaryDto>>>
{
    public async Task<Result<PagedResult<ProviderSummaryDto>>> Handle(
        GetProvidersQuery request,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<Provider>> providersResult = await providerRepository.GetVerifiedPagedAsync(
            request.Page, request.PageSize, cancellationToken);

        if (providersResult.IsFailure)
        {
            return providersResult.Error;
        }

        int totalCount = await providerRepository.CountVerifiedAsync(cancellationToken);

        IReadOnlyList<ProviderSummaryDto> items = providersResult.Value
            .Select(p => new ProviderSummaryDto(
                Id: p.Id,
                Name: p.Name,
                LogoUrl: p.LogoUrl,
                CuisineTags: p.CuisineTags.AsReadOnly(),
                Rating: p.Rating,
                MealCount: p.Meals.Count))
            .ToList()
            .AsReadOnly();

        return new PagedResult<ProviderSummaryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
        };
    }
}

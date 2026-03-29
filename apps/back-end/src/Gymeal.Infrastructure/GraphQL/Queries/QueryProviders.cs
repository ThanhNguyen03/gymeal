using Gymeal.Application.Common.Pagination;
using Gymeal.Application.Features.Providers.DTOs;
using Gymeal.Application.Features.Providers.Queries.GetProviders;
using Gymeal.Application.Features.Providers.Queries.GetProviderById;
using Gymeal.Domain.Common;
using MediatR;

namespace Gymeal.Infrastructure.GraphQL.Queries;

[ExtendObjectType("Query")]
public sealed class QueryProviders
{
    public async Task<PagedResult<ProviderSummaryDto>> GetProvidersAsync(
        int page,
        int pageSize,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<PagedResult<ProviderSummaryDto>> result = await mediator.Send(
            new GetProvidersQuery(page, pageSize), cancellationToken);

        return result.IsSuccess
            ? result.Value
            : throw new GraphQLException(result.Error.Message);
    }

    public async Task<ProviderDto> GetProviderByIdAsync(
        Guid id,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<ProviderDto> result = await mediator.Send(
            new GetProviderByIdQuery(id), cancellationToken);

        return result.IsSuccess
            ? result.Value
            : throw new GraphQLException(result.Error.Message);
    }
}

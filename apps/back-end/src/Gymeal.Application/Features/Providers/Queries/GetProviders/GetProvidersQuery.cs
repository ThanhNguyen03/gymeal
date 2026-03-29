using Gymeal.Application.Common.Pagination;
using Gymeal.Application.Features.Providers.DTOs;
using Gymeal.Domain.Common;
using MediatR;

namespace Gymeal.Application.Features.Providers.Queries.GetProviders;

public sealed record GetProvidersQuery(int Page, int PageSize) : IRequest<Result<PagedResult<ProviderSummaryDto>>>;

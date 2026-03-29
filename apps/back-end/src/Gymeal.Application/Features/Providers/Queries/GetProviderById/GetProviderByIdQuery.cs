using Gymeal.Application.Features.Providers.DTOs;
using Gymeal.Domain.Common;
using MediatR;

namespace Gymeal.Application.Features.Providers.Queries.GetProviderById;

public sealed record GetProviderByIdQuery(Guid Id) : IRequest<Result<ProviderDto>>;

using Gymeal.Application.Common.Behaviours;
using Gymeal.Application.Features.Providers.DTOs;
using Gymeal.Domain.Common;
using MediatR;

namespace Gymeal.Application.Features.Providers.Commands.VerifyProvider;

// SECURITY: This command is Admin-only. Authorization is enforced at the controller level
// via [Authorize(Roles = "Admin")]. The handler also re-validates the current user's role
// as a defense-in-depth measure.
public sealed record VerifyProviderCommand(Guid ProviderId) : IRequest<Result<ProviderDto>>, IAuditableCommand;

using Gymeal.Domain.Common;
using Gymeal.Domain.Interfaces.Services;
using MediatR;

namespace Gymeal.Application.Features.Auth.Commands.LogoutUser;

public sealed class LogoutUserCommandHandler(ITokenService tokenService)
    : IRequestHandler<LogoutUserCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        LogoutUserCommand request,
        CancellationToken cancellationToken)
    {
        // Revoke the refresh token in Redis so it cannot be reused.
        // If the token doesn't exist (already revoked / never valid), this is a no-op.
        await tokenService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);

        return true;
    }
}

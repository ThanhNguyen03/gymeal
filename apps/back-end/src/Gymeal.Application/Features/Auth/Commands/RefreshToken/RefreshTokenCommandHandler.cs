using Gymeal.Application.Common.Errors;
using Gymeal.Application.Features.Auth.DTOs;
using Gymeal.Domain.Entities;
using Gymeal.Domain.Interfaces.Repositories;
using Gymeal.Domain.Interfaces.Services;
using MediatR;

namespace Gymeal.Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    ITokenService tokenService,
    IDateTimeProvider dateTime) : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
{
    public async Task<Result<AuthResponseDto>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        Guid? userId = await tokenService.ValidateRefreshTokenAsync(request.Token, cancellationToken);
        if (userId is null)
        {
            return Error.Unauthorized("Refresh token is invalid or expired.");
        }

        User? user = await userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user is null)
        {
            return Error.Unauthorized("User not found.");
        }

        // Rotate refresh token — revoke old, issue new
        await tokenService.RevokeRefreshTokenAsync(request.Token, cancellationToken);

        string accessToken = tokenService.GenerateAccessToken(user);
        string newRefreshToken = tokenService.GenerateRefreshToken();

        await tokenService.StoreRefreshTokenAsync(user.Id, newRefreshToken, cancellationToken);

        return new AuthResponseDto(
            AccessToken: accessToken,
            RefreshToken: newRefreshToken,
            ExpiresAt: dateTime.UtcNow.AddMinutes(15),
            UserId: user.Id,
            Email: user.Email,
            Role: user.Role.ToString());
    }
}

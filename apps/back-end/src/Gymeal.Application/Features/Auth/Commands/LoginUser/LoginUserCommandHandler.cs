using Gymeal.Application.Common.Errors;
using Gymeal.Application.Features.Auth.DTOs;
using Gymeal.Domain.Entities;
using Gymeal.Domain.Interfaces.Repositories;
using Gymeal.Domain.Interfaces.Services;
using MediatR;

namespace Gymeal.Application.Features.Auth.Commands.LoginUser;

public sealed class LoginUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IDateTimeProvider dateTime) : IRequestHandler<LoginUserCommand, Result<AuthResponseDto>>
{
    public async Task<Result<AuthResponseDto>> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByEmailAsync(request.Email.ToLowerInvariant(), cancellationToken);

        // NOTE: We return the same error for "not found" and "wrong password" to prevent
        // user enumeration attacks — an attacker shouldn't be able to tell which one failed.
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Error.Unauthorized("Invalid email or password.");
        }

        string accessToken = tokenService.GenerateAccessToken(user);
        string refreshToken = tokenService.GenerateRefreshToken();

        await tokenService.StoreRefreshTokenAsync(user.Id, refreshToken, cancellationToken);

        return new AuthResponseDto(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            ExpiresAt: dateTime.UtcNow.AddMinutes(15),
            UserId: user.Id,
            Email: user.Email,
            Role: user.Role.ToString());
    }
}

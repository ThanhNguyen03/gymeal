using Gymeal.Application.Common.Errors;
using Gymeal.Application.Features.Auth.DTOs;
using Gymeal.Domain.Entities;
using Gymeal.Domain.Interfaces.Repositories;
using Gymeal.Domain.Interfaces.Services;
using MediatR;

namespace Gymeal.Application.Features.Auth.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IDateTimeProvider dateTime) : IRequestHandler<RegisterUserCommand, Result<AuthResponseDto>>
{
    public async Task<Result<AuthResponseDto>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        bool emailTaken = await userRepository.ExistsAsync(request.Email, cancellationToken);
        if (emailTaken)
        {
            return Error.Conflict($"Email '{request.Email}' is already registered.");
        }

        User user = new()
        {
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = passwordHasher.Hash(request.Password),
            CreatedAt = dateTime.UtcNow,
            UpdatedAt = dateTime.UtcNow,
        };

        UserProfile profile = new()
        {
            UserId = user.Id,
            FullName = request.FullName,
            UpdatedAt = dateTime.UtcNow,
        };

        user.Profile = profile;

        await userRepository.AddAsync(user, cancellationToken);

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

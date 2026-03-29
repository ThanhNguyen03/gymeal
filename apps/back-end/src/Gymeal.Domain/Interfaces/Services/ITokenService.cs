using Gymeal.Domain.Entities;

namespace Gymeal.Domain.Interfaces.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Task StoreRefreshTokenAsync(Guid userId, string token, CancellationToken cancellationToken = default);
    Task<Guid?> ValidateRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
}

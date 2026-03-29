using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Gymeal.Domain.Entities;
using Gymeal.Domain.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Gymeal.Infrastructure.Services;

/// <summary>
/// JWT RS256 access token generation + Redis-backed refresh token management.
/// </summary>
/// <remarks>
/// SECURITY: RS256 asymmetric signing — private key signs (back-end only),
/// public key verifies (can be shared with edge middleware or other services).
/// Refresh tokens are stored in Redis with TTL = 7 days. On logout, the key is
/// deleted immediately (revocation without DB lookup).
/// Reference: PLAN.md §7 (Application Security)
/// </remarks>
public sealed class ServiceRedisToken(IConfiguration configuration, IDistributedCache cache) : ITokenService
{
    private const int ACCESS_TOKEN_EXPIRY_MINUTES = 15;
    private const int REFRESH_TOKEN_EXPIRY_DAYS = 7;
    private const string REFRESH_KEY_PREFIX = "refresh:";

    public string GenerateAccessToken(User user)
    {
        RSA rsa = LoadPrivateKey();
        RsaSecurityKey privateKey = new(rsa);
        SigningCredentials credentials = new(privateKey, SecurityAlgorithms.RsaSha256);

        Claim[] claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        ];

        JwtSecurityToken token = new(
            issuer: configuration["JWT_ISSUER"],
            audience: configuration["JWT_AUDIENCE"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(ACCESS_TOKEN_EXPIRY_MINUTES),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        byte[] bytes = new byte[64];
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public async Task StoreRefreshTokenAsync(
        Guid userId,
        string token,
        CancellationToken cancellationToken = default)
    {
        DistributedCacheEntryOptions options = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(REFRESH_TOKEN_EXPIRY_DAYS),
        };

        await cache.SetStringAsync(
            $"{REFRESH_KEY_PREFIX}{token}",
            userId.ToString(),
            options,
            cancellationToken);
    }

    public async Task<Guid?> ValidateRefreshTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        string? value = await cache.GetStringAsync(
            $"{REFRESH_KEY_PREFIX}{token}",
            cancellationToken);

        if (value is null || !Guid.TryParse(value, out Guid userId))
        {
            return null;
        }

        return userId;
    }

    public async Task RevokeRefreshTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        await cache.RemoveAsync($"{REFRESH_KEY_PREFIX}{token}", cancellationToken);
    }

    private RSA LoadPrivateKey()
    {
        string base64 = configuration["JWT_PRIVATE_KEY_BASE64"]
            ?? throw new InvalidOperationException("JWT_PRIVATE_KEY_BASE64 is not configured.");

        byte[] keyBytes = Convert.FromBase64String(base64);
        RSA rsa = RSA.Create();
        rsa.ImportPkcs8PrivateKey(keyBytes, out _);
        return rsa;
    }
}

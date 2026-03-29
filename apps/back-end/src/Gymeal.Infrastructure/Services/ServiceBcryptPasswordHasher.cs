using Gymeal.Domain.Interfaces.Services;
using BC = BCrypt.Net.BCrypt;

namespace Gymeal.Infrastructure.Services;

/// <summary>
/// BCrypt password hasher with work factor 12.
/// Work factor 12 takes ~300ms on modern hardware — slow enough to resist brute-force.
/// </summary>
/// <remarks>
/// NOTE: BCrypt work factor 12 is the industry standard as of 2024.
/// Work factor 10 (default) is too fast on modern GPUs.
/// Work factor 14+ is noticeably slow for login UX.
/// </remarks>
public sealed class ServiceBcryptPasswordHasher : IPasswordHasher
{
    private const int WORK_FACTOR = 12;

    public string Hash(string password) =>
        BC.HashPassword(password, workFactor: WORK_FACTOR);

    public bool Verify(string password, string hash) =>
        BC.Verify(password, hash);
}

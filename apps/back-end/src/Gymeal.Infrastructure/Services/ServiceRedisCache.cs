using System.Text.Json;
using Gymeal.Domain.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Gymeal.Infrastructure.Services;

/// <summary>
/// Redis-backed cache service with prefix-based invalidation support.
/// </summary>
/// <remarks>
/// Key pattern: {prefix}:{normalized_key}
/// TTL default: 5 minutes.
/// Prefix invalidation (RemoveByPrefixAsync) requires direct StackExchange.Redis
/// IConnectionMultiplexer since IDistributedCache does not expose SCAN/DEL operations.
/// Reference: PLAN.md §4.4
/// </remarks>
public sealed class ServiceRedisCache(
    IDistributedCache cache,
    IConnectionMultiplexer redis) : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        string? json = await cache.GetStringAsync(key, cancellationToken);
        if (json is null) return default;

        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        DistributedCacheEntryOptions options = new()
        {
            AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(5),
        };

        string json = JsonSerializer.Serialize(value, JsonOptions);
        await cache.SetStringAsync(key, json, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await cache.RemoveAsync(key, cancellationToken);
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        // IDistributedCache does not support prefix scan — use StackExchange.Redis directly
        IServer server = redis.GetServer(redis.GetEndPoints().First());
        IDatabase db = redis.GetDatabase();

        await foreach (RedisKey key in server.KeysAsync(pattern: $"{prefix}*"))
        {
            await db.KeyDeleteAsync(key);
        }
    }
}

using Gymeal.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gymeal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── AI Service HttpClient (BFF typed client) ──────────────────────────
        services.AddHttpClient<AiServiceHttpClient>();

        // ── Redis ─────────────────────────────────────────────────────────────
        // NOTE: StackExchange.Redis connection registered here.
        // Used for caching meal search results and session data.
        string redisUrl = configuration["REDIS_URL"]
            ?? throw new InvalidOperationException("REDIS_URL is not configured.");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisUrl;
            options.InstanceName = "gymeal:";
        });

        // ── EF Core (PostgreSQL + pgvector) ───────────────────────────────────
        // NOTE: AppDbContext and entity configurations added in Sprint 1.
        // EF Core owns ALL migrations — including AI tables.
        // Never add Alembic. One migration owner, one source of truth.

        return services;
    }
}

using Gymeal.Application.Common.Interfaces;
using Gymeal.Domain.Interfaces.Repositories;
using Gymeal.Domain.Interfaces.Services;
using Gymeal.Infrastructure.Persistence;
using Gymeal.Infrastructure.Persistence.Repositories;
using Gymeal.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gymeal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── EF Core (PostgreSQL + pgvector) ───────────────────────────────────
        // DECISION: EF Core owns ALL migrations including AI tables.
        // Never add Alembic. One migration owner, one source of truth.
        // Reference: PLAN.md §10 (Migration Ownership Rule)
        string databaseUrl = configuration["DATABASE_URL"]
            ?? throw new InvalidOperationException("DATABASE_URL is not configured.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(databaseUrl, npgsql =>
                npgsql.EnableRetryOnFailure(maxRetryCount: 3)));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        // ── AI Service HttpClient (BFF typed client) ──────────────────────────
        services.AddHttpClient<ServiceAiHttpClient>();

        // ── Redis ─────────────────────────────────────────────────────────────
        string redisUrl = configuration["REDIS_URL"]
            ?? throw new InvalidOperationException("REDIS_URL is not configured.");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisUrl;
            options.InstanceName = "gymeal:";
        });

        // ── Repositories ──────────────────────────────────────────────────────
        services.AddScoped<IUserRepository, RepositoryUser>();

        // ── Domain services ───────────────────────────────────────────────────
        services.AddSingleton<IPasswordHasher, ServiceBcryptPasswordHasher>();
        services.AddSingleton<IDateTimeProvider, ServiceDateTimeProvider>();
        services.AddScoped<ITokenService, ServiceRedisToken>();

        return services;
    }
}

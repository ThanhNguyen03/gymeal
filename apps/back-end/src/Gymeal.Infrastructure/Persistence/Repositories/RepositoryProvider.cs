using Gymeal.Domain.Common;
using Gymeal.Domain.Entities;
using Gymeal.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gymeal.Infrastructure.Persistence.Repositories;

// NOTE: All DB methods return Result<T> to expose the failure path explicitly.
// Reference: RULE.md §5.6 (Repository Result Pattern)
public sealed class RepositoryProvider(AppDbContext context) : IProviderRepository
{
    public async Task<Result<Provider>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // N+1 prevention: Include Meals count (via navigation) in a single query
        Provider? provider = await context.Providers
            .Include(p => p.Meals)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return provider is null
            ? Error.NotFound("Provider", id)
            : Result<Provider>.Success(provider);
    }

    public async Task<Result<Provider>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        Provider? provider = await context.Providers
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        return provider is null
            ? Error.NotFound("Provider", userId)
            : Result<Provider>.Success(provider);
    }

    public async Task<Result<IReadOnlyList<Provider>>> GetVerifiedPagedAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Provider> providers = await context.Providers
            .Include(p => p.Meals)
            .Where(p => p.IsVerified)
            .OrderByDescending(p => p.Rating)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<Provider>>.Success(providers);
    }

    public async Task<Result<Provider>> AddAsync(Provider provider, CancellationToken cancellationToken = default)
    {
        await context.Providers.AddAsync(provider, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return Result<Provider>.Success(provider);
    }

    public async Task<Result<Provider>> UpdateAsync(Provider provider, CancellationToken cancellationToken = default)
    {
        context.Providers.Update(provider);
        await context.SaveChangesAsync(cancellationToken);
        return Result<Provider>.Success(provider);
    }

    public async Task<Result<Provider>> VerifyAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        Provider? provider = await context.Providers
            .FirstOrDefaultAsync(p => p.Id == providerId, cancellationToken);

        if (provider is null)
        {
            return Error.NotFound("Provider", providerId);
        }

        provider.IsVerified = true;
        await context.SaveChangesAsync(cancellationToken);
        return Result<Provider>.Success(provider);
    }

    public async Task<int> CountVerifiedAsync(CancellationToken cancellationToken = default) =>
        await context.Providers.CountAsync(p => p.IsVerified, cancellationToken);
}

using Gymeal.Domain.Common;
using Gymeal.Domain.Entities;
using Gymeal.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gymeal.Infrastructure.Persistence.Repositories;

// NOTE: All DB methods return Result<T> to expose the failure path explicitly.
// Callers pattern-match on IsSuccess/IsFailure — no silent null returns.
// Reference: RULE.md §5.6 (Repository Result Pattern)
public sealed class RepositoryUser(AppDbContext context) : IUserRepository
{
    public async Task<Result<User>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        User? user = await context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        return user is null
            ? Error.NotFound("User", id)
            : Result<User>.Success(user);
    }

    public async Task<Result<User>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        User? user = await context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        return user is null
            ? Error.NotFound("User", email)
            : Result<User>.Success(user);
    }

    public async Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default) =>
        await context.Users
            .AnyAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);

    public async Task<Result<User>> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return Result<User>.Success(user);
    }

    public async Task<Result<User>> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);
        return Result<User>.Success(user);
    }
}

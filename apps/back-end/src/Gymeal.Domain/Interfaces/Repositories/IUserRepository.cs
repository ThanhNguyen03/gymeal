using Gymeal.Domain.Common;
using Gymeal.Domain.Entities;

namespace Gymeal.Domain.Interfaces.Repositories;

// NOTE: All methods return Result<T> to standardize error handling at the database boundary.
// Callers pattern-match on IsSuccess/IsFailure instead of catching exceptions.
// ExistsAsync is exempt — it cannot fail in a business-logic sense.
// Reference: RULE.md §5.6 (Repository Result Pattern)
public interface IUserRepository
{
    Task<Result<User>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<User>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<Result<User>> AddAsync(User user, CancellationToken cancellationToken = default);
    Task<Result<User>> UpdateAsync(User user, CancellationToken cancellationToken = default);
}

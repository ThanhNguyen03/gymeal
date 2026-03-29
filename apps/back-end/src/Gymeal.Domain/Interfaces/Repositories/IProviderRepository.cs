using Gymeal.Domain.Common;
using Gymeal.Domain.Entities;

namespace Gymeal.Domain.Interfaces.Repositories;

// NOTE: All methods return Result<T> — callers pattern-match on IsSuccess/IsFailure.
// Reference: RULE.md §5.6 (Repository Result Pattern)
public interface IProviderRepository
{
    Task<Result<Provider>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<Provider>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<Provider>>> GetVerifiedPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<Provider>> AddAsync(Provider provider, CancellationToken cancellationToken = default);
    Task<Result<Provider>> UpdateAsync(Provider provider, CancellationToken cancellationToken = default);
    Task<Result<Provider>> VerifyAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<int> CountVerifiedAsync(CancellationToken cancellationToken = default);
}

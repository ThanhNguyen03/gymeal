using Gymeal.Domain.Common;
using Gymeal.Domain.Entities;

namespace Gymeal.Domain.Interfaces.Repositories;

// NOTE: All methods return Result<T> — callers pattern-match on IsSuccess/IsFailure.
// Reference: RULE.md §5.6 (Repository Result Pattern)
public interface IMealRepository
{
    Task<Result<Meal>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<Meal>>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<Meal>>> SearchAsync(string query, int limit, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<Meal>>> GetSimilarAsync(Guid mealId, int limit, CancellationToken cancellationToken = default);
    Task<Result<Meal>> AddAsync(Meal meal, CancellationToken cancellationToken = default);
    Task<Result<Meal>> UpdateAsync(Meal meal, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}

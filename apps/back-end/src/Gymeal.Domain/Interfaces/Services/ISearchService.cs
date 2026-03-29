using Gymeal.Domain.Entities;

namespace Gymeal.Domain.Interfaces.Services;

public interface ISearchService
{
    Task<IReadOnlyList<Meal>> SearchMealsAsync(string query, int limit, CancellationToken cancellationToken = default);
}

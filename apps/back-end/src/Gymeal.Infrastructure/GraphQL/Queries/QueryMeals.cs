using Gymeal.Application.Common.Pagination;
using Gymeal.Application.Features.Meals.DTOs;
using Gymeal.Application.Features.Meals.Queries.GetMeals;
using Gymeal.Application.Features.Meals.Queries.GetMealById;
using Gymeal.Application.Features.Meals.Queries.SearchMeals;
using Gymeal.Application.Features.Meals.Queries.GetSimilarMeals;
using Gymeal.Domain.Common;
using HotChocolate.Authorization;
using MediatR;

namespace Gymeal.Infrastructure.GraphQL.Queries;

[ExtendObjectType("Query")]
public sealed class QueryMeals
{
    public async Task<PagedResult<MealSummaryDto>> GetMealsPagedAsync(
        int page,
        int pageSize,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<PagedResult<MealSummaryDto>> result = await mediator.Send(
            new GetMealsQuery(page, pageSize), cancellationToken);

        return result.IsSuccess
            ? result.Value
            : throw new GraphQLException(result.Error.Message);
    }

    public async Task<MealDto> GetMealByIdAsync(
        Guid id,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<MealDto> result = await mediator.Send(
            new GetMealByIdQuery(id), cancellationToken);

        return result.IsSuccess
            ? result.Value
            : throw new GraphQLException(result.Error.Message);
    }

    public async Task<IReadOnlyList<MealSummaryDto>> SearchMealsAsync(
        string query,
        int limit,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<MealSummaryDto>> result = await mediator.Send(
            new SearchMealsQuery(query, limit), cancellationToken);

        return result.IsSuccess
            ? result.Value
            : throw new GraphQLException(result.Error.Message);
    }

    public async Task<IReadOnlyList<MealSummaryDto>> GetSimilarMealsAsync(
        Guid mealId,
        int first,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<MealSummaryDto>> result = await mediator.Send(
            new GetSimilarMealsQuery(mealId, first), cancellationToken);

        return result.IsSuccess
            ? result.Value
            : throw new GraphQLException(result.Error.Message);
    }
}

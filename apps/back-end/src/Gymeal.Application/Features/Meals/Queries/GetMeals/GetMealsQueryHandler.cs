using Gymeal.Application.Common.Pagination;
using Gymeal.Application.Features.Meals.DTOs;
using Gymeal.Domain.Common;
using Gymeal.Domain.Entities;
using Gymeal.Domain.Interfaces.Repositories;
using MediatR;

namespace Gymeal.Application.Features.Meals.Queries.GetMeals;

public sealed class GetMealsQueryHandler(IMealRepository mealRepository)
    : IRequestHandler<GetMealsQuery, Result<PagedResult<MealSummaryDto>>>
{
    public async Task<Result<PagedResult<MealSummaryDto>>> Handle(
        GetMealsQuery request,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<Meal>> mealsResult = await mealRepository.GetPagedAsync(
            request.Page, request.PageSize, cancellationToken);

        if (mealsResult.IsFailure)
        {
            return mealsResult.Error;
        }

        int totalCount = await mealRepository.CountAsync(cancellationToken);

        IReadOnlyList<MealSummaryDto> items = mealsResult.Value
            .Select(m => new MealSummaryDto(
                Id: m.Id,
                Name: m.Name,
                ImageUrl: m.ImageUrl,
                Category: m.Category,
                Calories: m.Calories,
                ProteinG: m.ProteinG,
                PriceInCents: m.PriceInCents,
                ProviderName: m.Provider.Name,
                IsAvailable: m.IsAvailable))
            .ToList()
            .AsReadOnly();

        return new PagedResult<MealSummaryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
        };
    }
}

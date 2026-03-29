using Gymeal.Application.Common.Pagination;
using Gymeal.Application.Features.Meals.DTOs;
using Gymeal.Domain.Common;
using MediatR;

namespace Gymeal.Application.Features.Meals.Queries.GetMeals;

public sealed record GetMealsQuery(int Page, int PageSize) : IRequest<Result<PagedResult<MealSummaryDto>>>;

using Gymeal.Application.Features.Meals.DTOs;
using Gymeal.Domain.Common;
using MediatR;

namespace Gymeal.Application.Features.Meals.Queries.GetMealById;

public sealed record GetMealByIdQuery(Guid Id) : IRequest<Result<MealDto>>;

using Gymeal.Application.Common.Behaviours;
using Gymeal.Domain.Common;
using MediatR;

namespace Gymeal.Application.Features.Meals.Commands.ToggleMealAvailability;

public sealed record ToggleMealAvailabilityCommand(Guid MealId) : IRequest<Result<bool>>, IAuditableCommand;

using Gymeal.Application.Common.Errors;
using Gymeal.Application.Features.Users.DTOs;
using MediatR;

namespace Gymeal.Application.Features.Users.Queries.GetCurrentUser;

public record GetCurrentUserQuery : IRequest<Result<UserProfileDto>>;

using Gymeal.Domain.Common;
using MediatR;

namespace Gymeal.Application.Features.Auth.Commands.LogoutUser;

public record LogoutUserCommand(string RefreshToken) : IRequest<Result<bool>>;

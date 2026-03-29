using Gymeal.Domain.Common;
using Gymeal.Application.Features.Auth.DTOs;
using MediatR;

namespace Gymeal.Application.Features.Auth.Commands.LoginUser;

public record LoginUserCommand(
    string Email,
    string Password) : IRequest<Result<AuthResponseDto>>;

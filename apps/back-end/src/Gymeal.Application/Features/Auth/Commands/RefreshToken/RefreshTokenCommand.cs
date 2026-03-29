using Gymeal.Domain.Common;
using Gymeal.Application.Features.Auth.DTOs;
using MediatR;

namespace Gymeal.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string Token) : IRequest<Result<AuthResponseDto>>;

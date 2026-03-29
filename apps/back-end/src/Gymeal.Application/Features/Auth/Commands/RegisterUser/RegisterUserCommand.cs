using Gymeal.Application.Common.Behaviours;
using Gymeal.Domain.Common;
using Gymeal.Application.Features.Auth.DTOs;
using MediatR;

namespace Gymeal.Application.Features.Auth.Commands.RegisterUser;

public record RegisterUserCommand(
    string Email,
    string Password,
    string FullName) : IRequest<Result<AuthResponseDto>>, IAuditableCommand;

using Gymeal.Application.Features.Users.Commands.UpdateUserProfile;
using Gymeal.Application.Features.Users.DTOs;
using Gymeal.Application.Features.Users.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gymeal.Presentation.Controllers;

[Route("api/v1/users")]
[Authorize]
public sealed class UsersController(ISender mediator) : ApiControllerBase
{
    /// <summary>Get the current authenticated user's profile.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        Result<UserProfileDto> result = await mediator.Send(new GetCurrentUserQuery(), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapError(result.Error);
    }

    /// <summary>Update the current user's fitness profile (partial update — only provided fields change).</summary>
    [HttpPut("me/profile")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateUserProfileCommand command,
        CancellationToken cancellationToken)
    {
        Result<UserProfileDto> result = await mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapError(result.Error);
    }

}

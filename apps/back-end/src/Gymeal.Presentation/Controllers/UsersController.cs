using Gymeal.Application.Common.Errors;
using Gymeal.Application.Features.Users.Commands.UpdateUserProfile;
using Gymeal.Application.Features.Users.DTOs;
using Gymeal.Application.Features.Users.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gymeal.Presentation.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public sealed class UsersController(ISender mediator) : ControllerBase
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

    private ObjectResult MapError(Error error)
    {
        (int status, string title) = error.Code switch
        {
            var c when c.EndsWith("NotFound", StringComparison.Ordinal) => (404, "Not Found"),
            var c when c.EndsWith("Unauthorized", StringComparison.Ordinal) => (401, "Unauthorized"),
            var c when c.EndsWith("Forbidden", StringComparison.Ordinal) => (403, "Forbidden"),
            var c when c.EndsWith("Failed", StringComparison.Ordinal) => (422, "Unprocessable Entity"),
            _ => (500, "Internal Server Error"),
        };

        return StatusCode(status, new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = error.Message,
            Extensions = { ["correlationId"] = HttpContext.Items["CorrelationId"] },
        });
    }
}

using Gymeal.Application.Features.Providers.Commands.VerifyProvider;
using Gymeal.Application.Features.Providers.DTOs;
using Gymeal.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gymeal.Presentation.Controllers;

// SECURITY: All endpoints require Admin role — enforced at controller level.
// Handler also re-validates role as defense-in-depth.
[Route("api/v1/admin")]
[Authorize(Roles = "Admin")]
public sealed class AdminController(ISender mediator) : ApiControllerBase
{
    /// <summary>Verify a provider account, making their meals visible in the public catalog.</summary>
    /// <response code="200">Provider successfully verified.</response>
    /// <response code="403">Caller is not an Admin.</response>
    /// <response code="404">Provider not found.</response>
    [HttpPatch("providers/{id:guid}/verify")]
    [ProducesResponseType(typeof(ProviderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyProvider(
        Guid id,
        CancellationToken cancellationToken)
    {
        Result<ProviderDto> result = await mediator.Send(
            new VerifyProviderCommand(id), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapError(result.Error);
    }
}

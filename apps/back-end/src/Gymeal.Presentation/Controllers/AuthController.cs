using Gymeal.Domain.Common;
using Gymeal.Application.Features.Auth.Commands.LoginUser;
using Gymeal.Application.Features.Auth.Commands.LogoutUser;
using Gymeal.Application.Features.Auth.Commands.RefreshToken;
using Gymeal.Application.Features.Auth.Commands.RegisterUser;
using Gymeal.Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gymeal.Presentation.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(ISender mediator) : ControllerBase
{
    /// <summary>Register a new customer account.</summary>
    /// <response code="201">Registration successful — auth cookies set.</response>
    /// <response code="409">Email already registered.</response>
    /// <response code="422">Validation failed.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        Result<AuthResponseDto> result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error);
        }

        SetAuthCookies(result.Value);

        return CreatedAtAction(nameof(Register), result.Value);
    }

    /// <summary>Login with email and password.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        Result<AuthResponseDto> result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error);
        }

        SetAuthCookies(result.Value);

        return Ok(result.Value);
    }

    /// <summary>Refresh the access token using the refresh token cookie.</summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        string? refreshToken = Request.Cookies["refresh_token"];
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized(new ProblemDetails
            {
                Status = 401,
                Title = "Unauthorized",
                Detail = "Refresh token cookie is missing.",
            });
        }

        Result<AuthResponseDto> result = await mediator.Send(
            new RefreshTokenCommand(refreshToken), cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error);
        }

        SetAuthCookies(result.Value);

        return Ok(result.Value);
    }

    /// <summary>Logout — revoke refresh token and clear cookies.</summary>
    [HttpDelete("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        string? refreshToken = Request.Cookies["refresh_token"];
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await mediator.Send(new LogoutUserCommand(refreshToken), cancellationToken);
        }

        ClearAuthCookies();

        return NoContent();
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private void SetAuthCookies(AuthResponseDto dto)
    {
        // SECURITY: HttpOnly prevents JS access. Secure ensures HTTPS only.
        // SameSite=Strict prevents CSRF. Path scopes the refresh token cookie.
        // Reference: PLAN.md §7 (Application Security)
        Response.Cookies.Append("access_token", dto.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            MaxAge = TimeSpan.FromMinutes(15),
            Path = "/",
        });

        Response.Cookies.Append("refresh_token", dto.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            MaxAge = TimeSpan.FromDays(7),
            Path = "/api/v1/auth/refresh",
        });
    }

    private void ClearAuthCookies()
    {
        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token", new CookieOptions { Path = "/api/v1/auth/refresh" });
    }

    private ObjectResult MapError(Error error)
    {
        (int status, string title) = error.Code switch
        {
            var c when c.EndsWith("NotFound", StringComparison.Ordinal) => (404, "Not Found"),
            var c when c.EndsWith("Unauthorized", StringComparison.Ordinal) => (401, "Unauthorized"),
            var c when c.EndsWith("Forbidden", StringComparison.Ordinal) => (403, "Forbidden"),
            var c when c.EndsWith("Conflict", StringComparison.Ordinal) => (409, "Conflict"),
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

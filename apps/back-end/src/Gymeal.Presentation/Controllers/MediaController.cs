using Gymeal.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gymeal.Presentation.Controllers;

[Route("api/v1/media")]
[Authorize]
public sealed class MediaController(IStorageService storageService) : ApiControllerBase
{
    private static readonly HashSet<string> AllowedFolders = ["meals", "providers", "avatars"];

    /// <summary>Get a server-side signed Cloudinary upload URL.</summary>
    /// <remarks>
    /// SECURITY: The signed URL is generated server-side — Cloudinary credentials never leave the server.
    /// Clients upload directly to Cloudinary using the signed URL; the server controls
    /// what can be uploaded (folder, file type) via signing parameters.
    /// </remarks>
    /// <response code="200">Returns the signed upload URL.</response>
    /// <response code="400">Invalid folder or file name.</response>
    /// <response code="401">Not authenticated.</response>
    [HttpPost("upload-url")]
    [ProducesResponseType(typeof(UploadUrlResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUploadUrl(
        [FromBody] UploadUrlRequestDto request,
        CancellationToken cancellationToken)
    {
        // SECURITY: Whitelist folders to prevent uploads to arbitrary Cloudinary paths
        if (!AllowedFolders.Contains(request.Folder))
        {
            return BadRequest(new ProblemDetails
            {
                Status = 400,
                Title = "Bad Request",
                Detail = $"Folder '{request.Folder}' is not allowed. Allowed folders: {string.Join(", ", AllowedFolders)}.",
            });
        }

        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            return BadRequest(new ProblemDetails
            {
                Status = 400,
                Title = "Bad Request",
                Detail = "FileName is required.",
            });
        }

        try
        {
            string url = await storageService.GetSignedUploadUrlAsync(
                request.Folder, request.FileName, cancellationToken);

            return Ok(new UploadUrlResponseDto(url));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = 400,
                Title = "Bad Request",
                Detail = ex.Message,
            });
        }
    }
}

public sealed record UploadUrlRequestDto(string Folder, string FileName);
public sealed record UploadUrlResponseDto(string UploadUrl);

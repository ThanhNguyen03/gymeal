using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Gymeal.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace Gymeal.Infrastructure.Services;

/// <summary>
/// Cloudinary signed upload URL generator.
/// </summary>
/// <remarks>
/// SECURITY: Signed URLs are generated server-side — Cloudinary credentials never leave the server.
/// Clients receive a short-lived signed URL and upload directly to Cloudinary.
/// File extension whitelist enforced here; size limits enforced in the upload preset.
/// Reference: PLAN.md §4.4
/// </remarks>
public sealed class ServiceCloudinaryStorage(IConfiguration configuration) : IStorageService
{
    // SECURITY: Only allow safe image formats — no SVG (XSS risk), no executables.
    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public Task<string> GetSignedUploadUrlAsync(
        string folder,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        string extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                $"File type '{extension}' is not allowed. Allowed types: jpg, jpeg, png, webp.");
        }

        string cloudName = configuration["CLOUDINARY_CLOUD_NAME"]
            ?? throw new InvalidOperationException("CLOUDINARY_CLOUD_NAME is not configured.");
        string apiKey = configuration["CLOUDINARY_API_KEY"]
            ?? throw new InvalidOperationException("CLOUDINARY_API_KEY is not configured.");
        string apiSecret = configuration["CLOUDINARY_API_SECRET"]
            ?? throw new InvalidOperationException("CLOUDINARY_API_SECRET is not configured.");

        Cloudinary cloudinary = new(new Account(cloudName, apiKey, apiSecret));

        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string publicId = $"{folder}/{Path.GetFileNameWithoutExtension(fileName)}_{timestamp}";

        SortedDictionary<string, object> parameters = new()
        {
            ["folder"] = folder,
            ["public_id"] = publicId,
            ["timestamp"] = timestamp,
        };

        string signature = cloudinary.Api.SignParameters(parameters);

        string signedUrl = $"https://api.cloudinary.com/v1_1/{cloudName}/image/upload" +
            $"?api_key={apiKey}" +
            $"&timestamp={timestamp}" +
            $"&public_id={Uri.EscapeDataString(publicId)}" +
            $"&signature={signature}";

        return Task.FromResult(signedUrl);
    }
}

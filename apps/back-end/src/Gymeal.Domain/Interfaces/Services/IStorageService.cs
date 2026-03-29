namespace Gymeal.Domain.Interfaces.Services;

public interface IStorageService
{
    /// <summary>
    /// Returns a server-side signed upload URL for direct client→storage uploads.
    /// SECURITY: Signed URLs prevent unauthorized uploads — the server controls
    /// what can be uploaded (folder, file type, size) without exposing storage credentials.
    /// </summary>
    Task<string> GetSignedUploadUrlAsync(string folder, string fileName, CancellationToken cancellationToken = default);
}

namespace Tawzeef.Domain.Contracts;

/// <summary>
/// Placed in Domain, not Application, for the same reason as <see cref="ICurrentUser"/>:
/// Infrastructure implements this and may only depend on Domain (BACKEND_GUIDELINES.md
/// non-negotiable #1). BUSINESS_REQUIREMENTS.md §17.3: private storage, never a publicly
/// readable path; every download's authorization check happens in the calling service, not here.
/// </summary>
public interface IFileStorage
{
    /// <returns>An opaque storage key — never a public URL or a predictable path.</returns>
    Task<string> SaveAsync(Stream content, string originalFileName, string contentType, CancellationToken ct = default);

    Task<Stream> OpenReadAsync(string storageKey, CancellationToken ct = default);

    Task DeleteAsync(string storageKey, CancellationToken ct = default);
}

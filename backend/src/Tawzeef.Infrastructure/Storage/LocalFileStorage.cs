using Microsoft.Extensions.Configuration;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Infrastructure.Storage;

/// <summary>
/// Dev/local-disk implementation of <see cref="IFileStorage"/>. The storage key is a random,
/// unguessable file name with no extension and no relation to the original file name — the
/// original name/content-type are recorded by the caller (e.g. JobSeekerDocument), not derivable
/// from the key. Swap for a cloud blob store before production; callers never see the difference.
/// </summary>
public class LocalFileStorage : IFileStorage
{
    private readonly string _rootPath;

    public LocalFileStorage(IConfiguration configuration)
    {
        _rootPath = configuration["FileStorage:LocalPath"] ?? Path.Combine(AppContext.BaseDirectory, "App_Data", "secure-files");
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(Stream content, string originalFileName, string contentType, CancellationToken ct = default)
    {
        var storageKey = Guid.NewGuid().ToString("N");
        await using var fileStream = File.Create(Path.Combine(_rootPath, storageKey));
        await content.CopyToAsync(fileStream, ct);
        return storageKey;
    }

    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken ct = default)
    {
        var path = ResolvePath(storageKey);
        return Task.FromResult<Stream>(File.OpenRead(path));
    }

    public Task DeleteAsync(string storageKey, CancellationToken ct = default)
    {
        var path = ResolvePath(storageKey);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    private string ResolvePath(string storageKey)
    {
        // storageKey is always our own Guid "N" format — reject anything else so a caller can
        // never turn this into a path-traversal read of an arbitrary file.
        if (!Guid.TryParseExact(storageKey, "N", out _))
            throw new ArgumentException("Invalid storage key.", nameof(storageKey));

        return Path.Combine(_rootPath, storageKey);
    }
}

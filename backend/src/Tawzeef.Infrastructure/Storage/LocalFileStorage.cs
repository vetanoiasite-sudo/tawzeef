using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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

    public LocalFileStorage(IConfiguration configuration, IHostEnvironment environment)
    {
        // Default anchors on ContentRootPath (the project/publish directory), never
        // AppContext.BaseDirectory — that resolves inside bin/<config>/<tfm>, which `dotnet clean`
        // or a fresh build wipes, silently deleting every uploaded document. FileStorage:LocalPath
        // still overrides this for a real deployment (see appsettings.json's comment / the setup
        // README) — e.g. a mounted volume or network share outside the deploy tree entirely.
        _rootPath = configuration["FileStorage:LocalPath"] ?? Path.Combine(environment.ContentRootPath, "App_Data", "secure-files");
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

using Tawzeef.Domain.Contracts;

namespace Tawzeef.Tests.TestHelpers;

/// <summary>A minimal in-memory stand-in for <see cref="IFileStorage"/> — these tests exercise
/// authorization/business-rule logic, not the real storage backend, so a plain dictionary is
/// enough rather than reaching for a mocking library this codebase doesn't otherwise use.</summary>
public class FakeFileStorage : IFileStorage
{
    public Task<string> SaveAsync(Stream content, string originalFileName, string contentType, CancellationToken ct = default) =>
        Task.FromResult(Guid.NewGuid().ToString());

    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken ct = default) =>
        Task.FromResult<Stream>(new MemoryStream([1, 2, 3]));

    public Task DeleteAsync(string storageKey, CancellationToken ct = default) => Task.CompletedTask;
}

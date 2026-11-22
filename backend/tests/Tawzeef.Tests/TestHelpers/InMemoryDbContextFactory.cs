using Microsoft.EntityFrameworkCore;
using Tawzeef.Infrastructure.Persistence;

namespace Tawzeef.Tests.TestHelpers;

/// <summary>Service-layer unit tests go straight at a real <see cref="AppDbContext"/> backed by
/// EF Core's InMemory provider — no DI container, no HTTP stack. A fresh, uniquely-named database
/// per call keeps tests isolated from each other even when run in parallel.</summary>
public static class InMemoryDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}

using Microsoft.EntityFrameworkCore;
using Tawzeef.Domain.Common;

namespace Tawzeef.Tests.Conventions;

/// <summary>
/// BACKEND_GUIDELINES.md §15 ("convention tests") / TESTING_DOCUMENTATION.md §3.7 (TC-RBAC-03/04).
/// These catch the mistakes code review misses — a layering violation or a missing EF mapping.
/// </summary>
public class ArchitectureTests
{
    [Fact]
    public void Application_has_no_reference_to_EfCore_or_SqlClient()
    {
        var applicationAssembly = typeof(Tawzeef.Application.Common.ServiceResult).Assembly;

        var forbidden = applicationAssembly.GetReferencedAssemblies()
            .Where(a => a.Name is not null &&
                        (a.Name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) ||
                         a.Name.StartsWith("Microsoft.Data.SqlClient", StringComparison.Ordinal)))
            .Select(a => a.Name)
            .ToList();

        Assert.True(forbidden.Count == 0, $"Tawzeef.Application must not reference: {string.Join(", ", forbidden)}");
    }

    [Fact]
    public void Every_IEntity_has_exactly_one_configuration()
    {
        var domainAssembly = typeof(IEntity).Assembly;
        var infrastructureAssembly = typeof(Tawzeef.Infrastructure.Persistence.AppDbContext).Assembly;

        var entityTypes = domainAssembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IEntity).IsAssignableFrom(t))
            .ToList();

        var configuredTypes = infrastructureAssembly.GetTypes()
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                .Select(i => i.GetGenericArguments()[0]))
            .ToList();

        var unconfigured = entityTypes.Where(e => configuredTypes.Count(c => c == e) != 1).Select(e => e.Name).ToList();

        Assert.True(unconfigured.Count == 0, $"Missing (or duplicate) IEntityTypeConfiguration for: {string.Join(", ", unconfigured)}");
    }
}

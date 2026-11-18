using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Tawzeef.Api.Authorization;

/// <summary>
/// BACKEND_GUIDELINES.md §12: "A naming convention is only default-deny if an unrecognized name
/// is an error rather than a gap." Run once at startup; throws (refuses to boot) if any
/// controller action's permission can't be resolved.
/// </summary>
public static class PermissionEndpointAudit
{
    public static void AssertEveryEndpointIsClassified(IServiceProvider services)
    {
        var provider = services.GetRequiredService<IActionDescriptorCollectionProvider>();
        var unresolved = provider.ActionDescriptors.Items
            .OfType<ControllerActionDescriptor>()
            // EndpointMetadata aggregates controller-level AND action-level attributes — unlike
            // MethodInfo.GetCustomAttributes, which misses a class-level [AllowAnonymous] (the
            // bug this fixes: AuthController declares it once, on the class).
            .Where(a => !a.EndpointMetadata.OfType<IAllowAnonymous>().Any())
            .Where(a => PermissionResolver.Resolve(a) is null)
            .Select(a => $"{a.ControllerName}.{a.ActionName}")
            .ToList();

        if (unresolved.Count > 0)
        {
            throw new InvalidOperationException(
                "The following endpoints have no resolvable permission (add a ControllerModuleMap " +
                $"entry, or a [RequiresPermission] attribute for a custom verb): {string.Join(", ", unresolved)}");
        }
    }
}

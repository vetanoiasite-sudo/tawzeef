using Microsoft.AspNetCore.Mvc.Controllers;

namespace Tawzeef.Api.Authorization;

/// <summary>
/// Resolves the permission code for an action: an explicit <see cref="RequiresPermissionAttribute"/>
/// wins; otherwise the Get/Create/Update/Deactivate naming convention (BACKEND_GUIDELINES.md §12).
/// Returns null when neither resolves — the caller must refuse, never silently allow.
/// </summary>
public static class PermissionResolver
{
    public static string? Resolve(ControllerActionDescriptor action)
    {
        var explicitCode = action.MethodInfo
            .GetCustomAttributes(typeof(RequiresPermissionAttribute), inherit: true)
            .Cast<RequiresPermissionAttribute>()
            .FirstOrDefault()?.Code;
        if (explicitCode is not null) return explicitCode;

        if (!ControllerModuleMap.Map.TryGetValue(action.ControllerName, out var module)) return null;

        var verb = ResolveVerb(action.ActionName);
        return verb is null ? null : $"{module}.{verb}";
    }

    private static string? ResolveVerb(string actionName) => actionName switch
    {
        _ when actionName.StartsWith("Get", StringComparison.Ordinal) => "view",
        _ when actionName.StartsWith("List", StringComparison.Ordinal) => "view",
        _ when actionName.StartsWith("Create", StringComparison.Ordinal) => "create",
        _ when actionName.StartsWith("Add", StringComparison.Ordinal) => "create",
        _ when actionName.StartsWith("Update", StringComparison.Ordinal) => "edit",
        _ when actionName.StartsWith("Edit", StringComparison.Ordinal) => "edit",
        _ when actionName.StartsWith("Deactivate", StringComparison.Ordinal) => "delete",
        _ when actionName.StartsWith("Delete", StringComparison.Ordinal) => "delete",
        _ => null,
    };
}

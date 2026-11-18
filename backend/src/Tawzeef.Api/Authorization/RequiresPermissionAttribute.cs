namespace Tawzeef.Api.Authorization;

/// <summary>
/// Explicit override for custom-verb actions the Get/Create/Update/Deactivate naming convention
/// can't resolve (approve, reject, suspend, publish, pay, …) — RBAC_PERMISSIONS.md §2.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class RequiresPermissionAttribute(string code) : Attribute
{
    public string Code { get; } = code;
}

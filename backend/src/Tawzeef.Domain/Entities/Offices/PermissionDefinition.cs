namespace Tawzeef.Domain.Entities.Offices;

/// <summary>
/// Seeded reference data (RBAC_PERMISSIONS.md §2) — never user-editable. <see cref="Code"/> is
/// "module.action", e.g. "jobseekers.approve".
/// </summary>
public class PermissionDefinition
{
    public string Code { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }
}

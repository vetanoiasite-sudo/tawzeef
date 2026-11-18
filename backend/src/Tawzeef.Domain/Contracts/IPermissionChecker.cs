namespace Tawzeef.Domain.Contracts;

/// <summary>
/// Answers "does this user hold this permission at all" — tenant/ownership scoping of *which*
/// records they may act on is a separate, service-layer concern (RBAC_PERMISSIONS.md §4), not
/// this check's job.
/// </summary>
public interface IPermissionChecker
{
    Task<bool> HasPermissionAsync(ICurrentUser user, string permissionCode, CancellationToken ct = default);
}

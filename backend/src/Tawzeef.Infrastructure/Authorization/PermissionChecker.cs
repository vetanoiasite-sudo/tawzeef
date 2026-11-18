using Microsoft.EntityFrameworkCore;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Enums;
using Tawzeef.Infrastructure.Persistence;

namespace Tawzeef.Infrastructure.Authorization;

/// <summary>
/// Covers SuperAdmin, OfficeAdmin (full set within their office) and OfficeStaff (data-driven
/// grants) — RBAC_PERMISSIONS.md §3. Employer/JobSeeker permission resolution is deliberately not
/// implemented yet: those roles have no <see cref="Tawzeef.Domain.Entities.Offices.UserOfficeMembership"/>
/// row, and their fixed per-feature capabilities are not yet codified because the features
/// themselves (JobAdvertisement, JobApplication, …) don't exist yet. Revisit this method, not the
/// matrix in RBAC_PERMISSIONS.md §3, when those land.
/// </summary>
public class PermissionChecker(AppDbContext context) : IPermissionChecker
{
    public async Task<bool> HasPermissionAsync(ICurrentUser user, string permissionCode, CancellationToken ct = default)
    {
        if (user.IsSuperAdmin) return true;
        if (user.Id is null || user.OfficeId is null) return false;

        var membership = await context.UserOfficeMemberships
            .Where(m => m.UserId == user.Id && m.OfficeId == user.OfficeId && m.MembershipStatus == MembershipStatus.Active)
            .Select(m => new { m.Id, m.Role })
            .FirstOrDefaultAsync(ct);

        if (membership is null) return false;
        if (membership.Role == OfficeRole.OfficeAdmin) return true;

        return await context.StaffPermissionGrants
            .AnyAsync(g => g.UserOfficeMembershipId == membership.Id && g.PermissionCode == permissionCode, ct);
    }
}

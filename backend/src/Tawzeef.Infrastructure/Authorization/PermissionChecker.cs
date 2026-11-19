using Microsoft.EntityFrameworkCore;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Enums;
using Tawzeef.Infrastructure.Persistence;

namespace Tawzeef.Infrastructure.Authorization;

/// <summary>
/// Covers SuperAdmin, OfficeAdmin (full set within their office) and OfficeStaff (data-driven
/// grants) — RBAC_PERMISSIONS.md §3.
/// </summary>
public class PermissionChecker(AppDbContext context) : IPermissionChecker
{
    /// <summary>The "...own" verbs (RBAC_PERMISSIONS.md §2) a JobSeeker holds simply by having a
    /// JobSeekerProfile — no UserOfficeMembership/StaffPermissionGrant involved. §4's ownership
    /// scoping (which row) is still enforced separately, in the service layer.</summary>
    private static readonly HashSet<string> JobSeekerOwnPermissions =
        ["jobseekers.viewown", "jobseekers.editown", "jobseekers.uploaddocuments", "jobseekers.submit"];

    /// <summary>Same idea, for an Employer holding an Employer row — mirrors JobSeekerOwnPermissions.</summary>
    private static readonly HashSet<string> EmployerOwnPermissions =
        ["employers.viewown", "employers.editown", "employers.uploaddocuments", "employers.submit"];

    /// <summary>RBAC_PERMISSIONS.md §3: granted to *either* a JobSeeker or an Employer simply by
    /// holding that role, scoped to exactly the two verbs complaint submission needs today.</summary>
    private static readonly HashSet<string> SharedOwnPermissions = ["complaints.create", "complaints.viewown"];

    public async Task<bool> HasPermissionAsync(ICurrentUser user, string permissionCode, CancellationToken ct = default)
    {
        if (user.IsSuperAdmin) return true;

        if (JobSeekerOwnPermissions.Contains(permissionCode))
        {
            if (user.Id is null) return false;
            return await context.JobSeekerProfiles.AnyAsync(p => p.UserId == user.Id, ct);
        }

        if (EmployerOwnPermissions.Contains(permissionCode))
        {
            if (user.Id is null) return false;
            return await context.Employers.AnyAsync(e => e.UserId == user.Id, ct);
        }

        if (SharedOwnPermissions.Contains(permissionCode))
        {
            if (user.Id is null) return false;
            return await context.JobSeekerProfiles.AnyAsync(p => p.UserId == user.Id, ct)
                || await context.Employers.AnyAsync(e => e.UserId == user.Id, ct);
        }

        if (user.Id is null || user.OfficeId is null) return false;

        var membership = await context.UserOfficeMemberships
            .Where(m => m.UserId == user.Id && m.OfficeId == user.OfficeId && m.MembershipStatus == MembershipStatus.Active)
            .Select(m => new { m.Id, m.Role })
            .FirstOrDefaultAsync(ct);

        if (membership is null) return false;
        if (membership.Role == OfficeRole.OfficeAdmin) return true;

        // RBAC_PERMISSIONS.md §0 item 8 / footnote 5: a suspension/cooperation-stop decision is
        // OfficeAdmin-only, period — never grantable to OfficeStaff, even with a StaffPermissionGrant
        // row for it (PermissionCatalogue.OfficeAdminOnly was previously unenforced dead code).
        if (PermissionCatalogue.OfficeAdminOnly.Contains(permissionCode)) return false;

        return await context.StaffPermissionGrants
            .AnyAsync(g => g.UserOfficeMembershipId == membership.Id && g.PermissionCode == permissionCode, ct);
    }
}

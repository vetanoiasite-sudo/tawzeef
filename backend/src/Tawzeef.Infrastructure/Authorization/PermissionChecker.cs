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
        [
            "jobseekers.viewown", "jobseekers.editown", "jobseekers.uploaddocuments", "jobseekers.submit",
            "jobseekers.respondemployed",
            "jobapplications.create", "jobapplications.viewown", "jobapplications.withdraw",
            "interviews.view", "interviews.respondfeedback",
            "hiringfollowupreports.respondown",
            "jobseekercourses.view", "jobseekercourses.create", "jobseekercourses.edit", "jobseekercourses.delete",
        ];

    /// <summary>Same idea, for an Employer holding an Employer row — mirrors JobSeekerOwnPermissions.
    /// jobadvertisements.view/jobapplications.view are safe to grant broadly here because the office's
    /// own review surfaces use a different code (jobadvertisements.review) — see
    /// OfficeJobAdvertisementsController; the service layer still scopes every query to this
    /// Employer's own company (RBAC_PERMISSIONS.md §4.2).</summary>
    private static readonly HashSet<string> EmployerOwnPermissions =
        [
            "employers.viewown", "employers.editown", "employers.uploaddocuments", "employers.submit",
            "jobadvertisements.view", "jobadvertisements.create", "jobadvertisements.edit",
            "jobadvertisements.submit", "jobadvertisements.renew", "jobadvertisements.close",
            "jobapplications.view", "jobapplications.updateoutcome",
            "interviews.view", "interviews.schedule", "interviews.edit",
            "interviewreports.create", "interviewreports.view",
            "hiringfollowupreports.view", "hiringfollowupreports.respondemployer",
            "applicantevaluationreports.create", "applicantevaluationreports.view",
        ];

    /// <summary>RBAC_PERMISSIONS.md §3: granted to *either* a JobSeeker or an Employer simply by
    /// holding that role.</summary>
    private static readonly HashSet<string> SharedOwnPermissions =
        [
            "complaints.create", "complaints.viewown",
            "suggestions.create", "suggestions.viewown",
            "notifications.viewown",
        ];

    public async Task<bool> HasPermissionAsync(ICurrentUser user, string permissionCode, CancellationToken ct = default)
    {
        if (user.IsSuperAdmin) return true;

        // Office staff (OfficeId set) always resolve through membership, even for a code — like
        // jobapplications.view/jobadvertisements.view/jobapplications.updateoutcome — that is also
        // in one of the own-permission hashsets below for a bare Employer/JobSeeker account (no
        // UserOfficeMembership ever exists for those roles, so this never misroutes them): without
        // this office-first check, an OfficeAdmin calling a shared-code endpoint would incorrectly
        // fall into e.g. the Employer-ownership check below and get a false "not an employer" deny.
        if (user.Id is not null && user.OfficeId is not null)
            return await HasOfficeMembershipPermissionAsync(user.Id.Value, user.OfficeId.Value, permissionCode, ct);

        if (user.Id is null) return false;

        // Resolve which role this bare (non-office) account actually holds FIRST, then check that
        // role's own-permission set — never the reverse. Checking "is this code in some hashset"
        // before "which role is this" breaks the moment a code (like interviews.view, shared by
        // Employer and JobSeeker per RBAC_PERMISSIONS.md §3) appears in more than one hashset: an
        // Employer calling with that code would incorrectly hit the JobSeeker branch first and get
        // a false deny, since they have no JobSeekerProfile row. A user holds exactly one of these
        // roles (or none), so this existence check is also the correct ownership gate, not just a
        // performance shortcut.
        if (await context.JobSeekerProfiles.AnyAsync(p => p.UserId == user.Id, ct))
            return JobSeekerOwnPermissions.Contains(permissionCode) || SharedOwnPermissions.Contains(permissionCode);

        if (await context.Employers.AnyAsync(e => e.UserId == user.Id, ct))
            return EmployerOwnPermissions.Contains(permissionCode) || SharedOwnPermissions.Contains(permissionCode);

        return false;
    }

    /// <summary>RBAC_PERMISSIONS.md §3's matrix: every role gets a plain checkmark (✅ own) here,
    /// OfficeStaff included — unlike every other office permission, this one needs no
    /// StaffPermissionGrant row at all, just active membership. "dashboards.view" joins this set
    /// because the office overview page (counts drawn from whatever queues the viewer already has
    /// access to) is baseline for any active office member, not a selectively-grantable review
    /// permission.</summary>
    private static readonly HashSet<string> UniversalOfficeMemberPermissions = ["notifications.viewown", "dashboards.view"];

    private async Task<bool> HasOfficeMembershipPermissionAsync(Guid userId, Guid officeId, string permissionCode, CancellationToken ct)
    {
        var membership = await context.UserOfficeMemberships
            .Where(m => m.UserId == userId && m.OfficeId == officeId && m.MembershipStatus == MembershipStatus.Active)
            .Select(m => new { m.Id, m.Role })
            .FirstOrDefaultAsync(ct);

        if (membership is null) return false;
        if (membership.Role == OfficeRole.OfficeAdmin) return true;
        if (UniversalOfficeMemberPermissions.Contains(permissionCode)) return true;

        // RBAC_PERMISSIONS.md §0 item 8 / footnote 5: a suspension/cooperation-stop decision is
        // OfficeAdmin-only, period — never grantable to OfficeStaff, even with a StaffPermissionGrant
        // row for it (PermissionCatalogue.OfficeAdminOnly was previously unenforced dead code).
        if (PermissionCatalogue.OfficeAdminOnly.Contains(permissionCode)) return false;

        return await context.StaffPermissionGrants
            .AnyAsync(g => g.UserOfficeMembershipId == membership.Id && g.PermissionCode == permissionCode, ct);
    }
}

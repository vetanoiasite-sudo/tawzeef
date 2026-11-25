namespace Tawzeef.Domain.Common;

public sealed record PermissionSeed(string Code, string Module, string Action, string? Description = null);

/// <summary>
/// The seeded permission catalogue — RBAC_PERMISSIONS.md §2. Seeded into
/// <see cref="Tawzeef.Domain.Entities.Offices.PermissionDefinition"/> by the
/// infrastructure's DbInitializer; never edited at runtime.
/// </summary>
public static class PermissionCatalogue
{
    private static readonly (string Module, string[] Actions)[] ModuleActions =
    [
        ("offices", ["view", "create", "edit", "deactivate"]),
        ("officeusers", ["view", "invite", "edit", "deactivate"]),
        ("jobseekers", ["view", "viewown", "viewdocuments", "editown", "uploaddocuments", "submit", "review", "approve", "reject", "suspend", "reinstate"]),
        ("employers", ["view", "viewown", "viewdocuments", "editown", "uploaddocuments", "submit", "review", "approve", "reject", "suspend", "reinstate"]),
        ("jobadvertisements", ["view", "create", "edit", "submit", "review", "approve", "reject", "publish", "pause", "close", "renew"]),
        ("advertisementpayments", ["view", "record", "refund"]),
        ("jobapplications", ["view", "viewown", "create", "withdraw", "updateoutcome"]),
        // ^ unchanged — already had the exact action lists job advertisements/applications need.
        ("applicantdata", ["viewgated"]),
        ("interviews", ["view", "schedule", "edit"]),
        ("interviewreports", ["view", "create"]),
        ("hiringfollowupreports", ["view", "create"]),
        ("applicantevaluationreports", ["view", "create"]),
        ("complaints", ["view", "viewown", "create", "triage", "resolve", "decide"]),
        ("suggestions", ["view", "viewown", "create", "respond"]),
        ("officedecisions", ["view", "create"]),
        ("notifications", ["viewown"]),
        ("auditlogs", ["view"]),
        ("dashboards", ["view"]),
        ("platformsettings", ["view", "edit"]),
        // Job seeker's own course history — self-service only, never a StaffPermissionGrant
        // (PermissionChecker.JobSeekerOwnPermissions grants it directly, same as jobseekers.*'s
        // own-verbs). Replaces the earlier office-authored "courses" concept entirely (2026-11-25).
        ("jobseekercourses", ["view", "create", "edit", "delete"]),
        ("banners", ["view", "create", "pause"]),
    ];

    public static IReadOnlyList<PermissionSeed> All { get; } =
        ModuleActions
            .SelectMany(m => m.Actions.Select(a => new PermissionSeed($"{m.Module}.{a}", m.Module, a)))
            .ToList();

    /// <summary>Never grantable to OfficeStaff — RBAC_PERMISSIONS.md footnotes 1 and 5.</summary>
    public static readonly string[] OfficeAdminOnly =
    [
        "officeusers.view", "officeusers.invite", "officeusers.edit", "officeusers.deactivate",
        "complaints.decide",
        "jobseekers.suspend", "jobseekers.reinstate",
        "employers.suspend", "employers.reinstate",
        // Banners (2026-11-24): explicitly OfficeAdmin-only, never a StaffPermissionGrant — the
        // office owner publishes these personally, same reasoning as officeusers.*. (Courses used
        // to be here too, before 2026-11-25's rework into a job-seeker self-service feature.)
        "banners.view", "banners.create", "banners.pause",
    ];
}

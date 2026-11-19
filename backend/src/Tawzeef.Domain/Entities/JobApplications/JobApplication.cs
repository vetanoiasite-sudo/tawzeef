using Tawzeef.Domain.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.JobApplications;

/// <summary>DATABASE_DOCUMENTATION.md §6, WORKFLOWS_AND_STATE_MACHINES.md §7. Two independent
/// status columns by design (BUSINESS_REQUIREMENTS.md §8) — never derive one from the other via a
/// lookup; the service keeps them in sync per the confirmed mapping table.</summary>
public class JobApplication : IEntity
{
    public Guid Id { get; set; }

    public Guid OfficeId { get; set; }
    public Guid JobAdvertisementId { get; set; }
    public Guid JobSeekerId { get; set; }

    public EmployerApplicationOutcome EmployerFacingStatus { get; set; } = EmployerApplicationOutcome.NotYetContacted;
    public JobSeekerApplicationStatus JobSeekerFacingStatus { get; set; } = JobSeekerApplicationStatus.NotContacted;

    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastStatusChangedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Confirmed (RBAC_PERMISSIONS.md §0 item 14): terminal for this row once set — a
    /// withdrawn application is never reactivated, only re-applied as a new row.</summary>
    public DateTime? WithdrawnAt { get; set; }

    /// <summary>WORKFLOWS_AND_STATE_MACHINES.md §9.1's flow diagram references this field directly
    /// (not listed under DATABASE_DOCUMENTATION.md §6's JobApplication columns — added here to
    /// support it). Stamped by HiringFollowUpReportService when the office creates the follow-up
    /// report — that POST is literally how a hire gets recorded (no separate "mark hired" action is
    /// documented).</summary>
    public DateTime? HiredAt { get; set; }

    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;
}

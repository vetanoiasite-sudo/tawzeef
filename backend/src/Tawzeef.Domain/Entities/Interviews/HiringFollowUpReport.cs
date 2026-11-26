using Tawzeef.Domain.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.Interviews;

/// <summary>DATABASE_DOCUMENTATION.md §6 — the "second report", one month after hiring
/// (BUSINESS_REQUIREMENTS.md §10.2). Originally written by the office only, mandatory for every hire
/// made through a platform JobApplication (JobApplicationId set) — creating this row is how the
/// office marks a JobApplication "hired" (API_DOCUMENTATION.md §3.9's POST route, "create, on
/// marking hired"). Extended 2026-11-26: a job seeker can also self-report finding a job outside any
/// specific JobApplication (JobApplicationId null, JobSeekerOfficeApplicationId set instead), and
/// answer this report themselves from their own notifications — see JobSeekerFollowUpService.</summary>
public class HiringFollowUpReport : IEntity
{
    public Guid Id { get; set; }

    public Guid OfficeId { get; set; }
    public Guid? JobApplicationId { get; set; }
    public Guid? JobSeekerOfficeApplicationId { get; set; }

    public DateTime HiredAt { get; set; }
    public DateTime DueAt { get; set; }
    public FollowUpReportStatus Status { get; set; } = FollowUpReportStatus.PendingDue;

    public bool? StillEmployed { get; set; }
    public bool? EmployerSatisfied { get; set; }
    public bool? JobSeekerSatisfied { get; set; }
    public string? Notes { get; set; }

    /// <summary>True when the job seeker answered this themselves from a notification, false/absent
    /// when the office filled it in manually — display-only, never changes who is allowed to call
    /// CompleteAsync.</summary>
    public bool SelfReported { get; set; }
    public DateTime? LastNotifiedAt { get; set; }

    /// <summary>The employer's own reply to "are you satisfied with this hire?" — independent of the
    /// job seeker's StillEmployed/Notes/Status above, and only ever asked when JobApplicationId is
    /// set (a self-reported hire has no specific employer to ask). Added 2026-11-26, see
    /// EmployerFollowUpService.</summary>
    public string? EmployerNotes { get; set; }
    public DateTime? EmployerRespondedAt { get; set; }
    public DateTime? EmployerLastNotifiedAt { get; set; }

    /// <summary>Set once, the first time this report goes severely overdue (DueAt plus two full
    /// reminder cycles) with no job seeker reply — escalates to the office so a human calls
    /// directly, instead of the system nagging forever. Never cleared/reset.</summary>
    public DateTime? OfficeEscalatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }
    public Guid? CompletedBy { get; set; }

    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;
}

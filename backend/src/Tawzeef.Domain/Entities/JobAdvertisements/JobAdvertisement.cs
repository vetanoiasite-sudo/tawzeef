using Tawzeef.Domain.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.JobAdvertisements;

/// <summary>DATABASE_DOCUMENTATION.md §5, WORKFLOWS_AND_STATE_MACHINES.md §5. City is still free
/// text (DATABASE_DOCUMENTATION.md §7.1's City lookup table is a proposed, not-yet-confirmed
/// addition) — same deviation as Employer.City/JobSeekerProfile.City. Category is split in two,
/// confirmed 2026-11-23: <see cref="Category"/> stays free text — now specifically the employer's
/// own suggestion, typed at creation, never authoritative — and <see cref="CategoryId"/> is the
/// office-assigned real category (<see cref="JobCategory"/>), set during review and required
/// before approval. Public search/display use <see cref="CategoryId"/> once set; the employer's
/// free text is for the office's reference only.</summary>
public class JobAdvertisement : IEntity
{
    public Guid Id { get; set; }

    public Guid OfficeId { get; set; }
    public Guid EmployerId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ExperienceRequirement { get; set; }
    public string? WorkingHours { get; set; }
    public string? Address { get; set; }

    /// <summary>The employer's own free-text guess at creation time — never authoritative, never
    /// used for search/display once <see cref="CategoryId"/> is assigned. See the class doc comment.</summary>
    public string? Category { get; set; }

    /// <summary>Office-assigned, set during review (JobAdvertisementReviewService.AssignCategoryAsync)
    /// — required before <see cref="JobAdvertisementStatus.UnderReview"/> can move to Approved.</summary>
    public Guid? CategoryId { get; set; }

    public string? City { get; set; }
    public EmploymentType? EmploymentType { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public int VacancyCount { get; set; } = 1;

    public JobAdvertisementStatus Status { get; set; } = JobAdvertisementStatus.Draft;
    public DateTime? SubmittedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }

    public DateTime? PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    /// <summary>Last time the employer was reminded about applicants still NotYetContacted on this
    /// ad — null means never. Only reset by a new reminder tick, never cleared; the reminder simply
    /// stops firing once no NotYetContacted applicant remains or the ad leaves Published.</summary>
    public DateTime? PendingApplicantsLastNotifiedAt { get; set; }

    /// <summary>Set once, a few days before ExpiresAt, so the employer gets one "renew before it
    /// expires" nudge — never repeated, unlike PendingApplicantsLastNotifiedAt above.</summary>
    public DateTime? ExpiryReminderSentAt { get; set; }

    /// <summary>Set only on a renewal's new row — WORKFLOWS_AND_STATE_MACHINES.md §5 confirmed
    /// item 17: renewal always creates a new advertisement, never an extension of the old one.</summary>
    public Guid? RenewedFromAdvertisementId { get; set; }

    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;
}

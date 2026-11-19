using Tawzeef.Domain.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.JobAdvertisements;

/// <summary>DATABASE_DOCUMENTATION.md §5, WORKFLOWS_AND_STATE_MACHINES.md §5. Category/City are
/// free text (DATABASE_DOCUMENTATION.md §7.1's JobCategory/City lookup tables are a proposed,
/// not-yet-confirmed addition) — same deviation as Employer.City/JobSeekerProfile.City.</summary>
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
    public string? Category { get; set; }
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

    /// <summary>Set only on a renewal's new row — WORKFLOWS_AND_STATE_MACHINES.md §5 confirmed
    /// item 17: renewal always creates a new advertisement, never an extension of the old one.</summary>
    public Guid? RenewedFromAdvertisementId { get; set; }

    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;
}

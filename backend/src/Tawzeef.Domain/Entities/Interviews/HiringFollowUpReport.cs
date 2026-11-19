using Tawzeef.Domain.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.Interviews;

/// <summary>DATABASE_DOCUMENTATION.md §6 — the "second report", one month after hiring
/// (BUSINESS_REQUIREMENTS.md §10.2). Confirmed written by the office, mandatory for every hire.
/// Creating this row is also how the office marks a JobApplication "hired" — API_DOCUMENTATION.md
/// §3.9's POST route is annotated "create, on marking hired".</summary>
public class HiringFollowUpReport : IEntity
{
    public Guid Id { get; set; }

    public Guid OfficeId { get; set; }
    public Guid JobApplicationId { get; set; }

    public DateTime HiredAt { get; set; }
    public DateTime DueAt { get; set; }
    public FollowUpReportStatus Status { get; set; } = FollowUpReportStatus.PendingDue;

    public bool? StillEmployed { get; set; }
    public bool? EmployerSatisfied { get; set; }
    public bool? JobSeekerSatisfied { get; set; }
    public string? Notes { get; set; }

    public DateTime? CompletedAt { get; set; }
    public Guid? CompletedBy { get; set; }

    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;
}

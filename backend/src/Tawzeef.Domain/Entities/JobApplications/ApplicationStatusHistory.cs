using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.JobApplications;

/// <summary>DATABASE_DOCUMENTATION.md §6. Append-only audit trail — deliberately not an IEntity,
/// same convention as OfficeDecision: ChangedBy/ChangedAt are its own stamp, no soft-delete.
/// StatusDomain discriminates which of JobApplication's two status columns OldStatus/NewStatus
/// refer to, since both tracks funnel into this one table.</summary>
public class ApplicationStatusHistory
{
    public Guid Id { get; set; }

    public Guid JobApplicationId { get; set; }
    public ApplicationStatusDomain StatusDomain { get; set; }
    public byte OldStatus { get; set; }
    public byte NewStatus { get; set; }

    public Guid ChangedBy { get; set; }
    public ApplicationActorRole ActorRole { get; set; }
    public string? Notes { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}

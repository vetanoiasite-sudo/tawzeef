using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.Offices;

/// <summary>DATABASE_DOCUMENTATION.md §"OfficeDecision" — append-only audit of the two enforcement
/// actions BUSINESS_REQUIREMENTS.md requires an explicit office approval for (§11.3, §11.4). Not
/// an IEntity: no soft-delete, no modification — DecidedBy/DecidedAt are its own audit stamp and
/// a row, once written, is never changed.</summary>
public class OfficeDecision
{
    public Guid Id { get; set; }

    public Guid OfficeId { get; set; }

    public OfficeDecisionTargetType TargetType { get; set; }
    public Guid TargetId { get; set; }

    public OfficeDecisionType DecisionType { get; set; }

    /// <summary>Mandatory — this table exists specifically so a suspension is never silent.</summary>
    public string Reason { get; set; } = string.Empty;

    public Guid DecidedBy { get; set; }
    public DateTime DecidedAt { get; set; } = DateTime.UtcNow;
}

using Tawzeef.Domain.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.Complaints;

/// <summary>BUSINESS_REQUIREMENTS.md §11.2 field list. OfficeId is the subject's own office
/// (RBAC_PERMISSIONS.md §0 item 4: exactly one office per JobSeeker/Employer account) — not
/// necessarily the sender's, since a complaint can cross roles (a JobSeeker complaining about an
/// Employer or vice versa).</summary>
public class Complaint : IEntity
{
    public Guid Id { get; set; }

    public Guid OfficeId { get; set; }
    public Guid SubmittedByUserId { get; set; }
    public ComplaintSenderType SenderType { get; set; }

    public ComplaintSubjectType SubjectType { get; set; }
    public Guid SubjectId { get; set; }

    public string ComplaintText { get; set; } = string.Empty;

    public ComplaintStatus Status { get; set; } = ComplaintStatus.New;
    public ComplaintValidityDecision ValidityDecision { get; set; } = ComplaintValidityDecision.Pending;
    public string? ResolutionNotes { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;
}

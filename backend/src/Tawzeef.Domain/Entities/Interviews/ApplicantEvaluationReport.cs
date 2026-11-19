using Tawzeef.Domain.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.Interviews;

/// <summary>DATABASE_DOCUMENTATION.md §6 (§9/§10.3). One per JobApplication — a re-evaluation
/// replaces Outcome/Notes on the existing row rather than creating a second one.</summary>
public class ApplicantEvaluationReport : IEntity
{
    public Guid Id { get; set; }

    public Guid OfficeId { get; set; }
    public Guid JobApplicationId { get; set; }
    public Guid EmployerId { get; set; }

    public ApplicantEvaluationOutcome Outcome { get; set; }
    public string? Notes { get; set; }

    public Guid SubmittedBy { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;
}

using Tawzeef.Domain.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.Employers;

/// <summary>An employer may submit to several offices (or all of them), and each office reviews
/// the same shared EmployerDocument set independently — confirmed 2026-11-24, replacing the old
/// single Employer.OfficeId. The employer may only post an advertisement under an office where this
/// row's VerificationStatus is Approved and CooperationStatus is Active (JobAdvertisementService).</summary>
public class EmployerOfficeApplication : IEntity
{
    public Guid Id { get; set; }

    public Guid EmployerId { get; set; }
    public Guid OfficeId { get; set; }

    public EmployerVerificationStatus VerificationStatus { get; set; } = EmployerVerificationStatus.Draft;
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }

    public EmployerCooperationStatus CooperationStatus { get; set; } = EmployerCooperationStatus.Active;
    public DateTime? CooperationStoppedAt { get; set; }
    public string? CooperationStoppedReason { get; set; }

    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;
}

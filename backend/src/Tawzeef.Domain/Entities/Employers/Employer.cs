using Tawzeef.Domain.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.Employers;

/// <summary>DATABASE_DOCUMENTATION.md §5. Created as a Draft stub at registration; fields beyond
/// LegalName/DisplayName/RepresentativeName are filled in by the "complete company profile" step
/// (not built yet) — same deferral as JobSeekerProfile.</summary>
public class Employer : IEntity
{
    public Guid Id { get; set; }

    public Guid OfficeId { get; set; }
    public Guid UserId { get; set; }

    public string LegalName { get; set; } = string.Empty;
    /// <summary>Shown publicly on job listings — never contact info (BUSINESS_REQUIREMENTS.md §6.2).</summary>
    public string DisplayName { get; set; } = string.Empty;
    public string? BusinessActivity { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    /// <summary>Private — never serialized into a public job DTO.</summary>
    public string? CompanyEmail { get; set; }
    /// <summary>Private — same.</summary>
    public string? CompanyPhone { get; set; }
    public string? RepresentativeName { get; set; }
    public string? RepresentativeRole { get; set; }

    public EmployerVerificationStatus VerificationStatus { get; set; } = EmployerVerificationStatus.Draft;
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }

    public EmployerCooperationStatus CooperationStatus { get; set; } = EmployerCooperationStatus.Active;
    public string? CooperationStoppedReason { get; set; }
    public DateTime? CooperationStoppedAt { get; set; }

    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;
}

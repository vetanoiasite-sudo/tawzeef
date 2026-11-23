using Tawzeef.Domain.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.JobSeekers;

/// <summary>DATABASE_DOCUMENTATION.md §4. Created as a Draft stub at registration; fields beyond
/// FullName/MobileNumber are filled in by the "complete profile" step (not built yet).</summary>
public class JobSeekerProfile : IEntity
{
    public Guid Id { get; set; }

    public Guid OfficeId { get; set; }
    public Guid UserId { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Education { get; set; }
    public byte? YearsOfExperience { get; set; }
    public string? Skills { get; set; }
    public string? CareerSummary { get; set; }
    public string? JobTitle { get; set; }

    /// <summary>Optional profile photo — never required, no office review workflow attached
    /// (unlike JobSeekerDocument). Opaque storage key from IFileStorage, same as a document's
    /// SecureFileReference (BUSINESS_REQUIREMENTS.md §17.3) — never a public URL. Visible only to
    /// the job seeker themselves and office staff reviewing the profile; never exposed to an
    /// employer.</summary>
    public string? PhotoStorageKey { get; set; }
    public string? PhotoOriginalFileName { get; set; }
    public string? PhotoContentType { get; set; }

    public JobSeekerReviewStatus ReviewStatus { get; set; } = JobSeekerReviewStatus.Draft;
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? SuspendedAt { get; set; }
    public string? SuspensionReason { get; set; }

    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;
}

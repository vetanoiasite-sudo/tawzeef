using Tawzeef.Domain.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.JobSeekers;

/// <summary>DATABASE_DOCUMENTATION.md §4. History is kept per (JobSeekerId, DocumentType) —
/// the active document of each type is the most recent row with IsActive = true.</summary>
public class JobSeekerDocument : IEntity
{
    public Guid Id { get; set; }

    public Guid OfficeId { get; set; }
    public Guid JobSeekerId { get; set; }

    public JobSeekerDocumentType DocumentType { get; set; }

    /// <summary>Opaque storage key from IFileStorage — never a public URL (BUSINESS_REQUIREMENTS.md §17.3).</summary>
    public string SecureFileReference { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public int FileSizeBytes { get; set; }

    public DocumentReviewStatus ReviewStatus { get; set; } = DocumentReviewStatus.Pending;
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;
}

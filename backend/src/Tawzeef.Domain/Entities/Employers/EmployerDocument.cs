using Tawzeef.Domain.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.Employers;

/// <summary>Mirrors JobSeekerDocument exactly (WORKFLOWS_AND_STATE_MACHINES.md §2: "one state
/// machine, two tables, same shape"). History is kept per (EmployerId, DocumentType) — the active
/// document of each type is the most recent row with IsActive = true.</summary>
public class EmployerDocument : IEntity
{
    public Guid Id { get; set; }

    public Guid EmployerId { get; set; }

    public EmployerDocumentType DocumentType { get; set; }

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

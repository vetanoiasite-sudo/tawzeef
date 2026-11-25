using Tawzeef.Domain.Common;

namespace Tawzeef.Domain.Entities.JobSeekers;

/// <summary>A course/training the job seeker took on their own — self-reported, no office review
/// workflow (replaces the earlier office-authored public Course concept entirely, 2026-11-25
/// product decision: "courses" now means the job seeker's own course history, not something an
/// office advertises). The certificate is optional — not every course was completed with a scan
/// of the certificate on hand.</summary>
public class JobSeekerCourse : IEntity
{
    public Guid Id { get; set; }
    public Guid JobSeekerId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Institution { get; set; } = string.Empty;
    public string? WhatLearned { get; set; }

    /// <summary>Opaque storage key from IFileStorage — never a public URL, same as a profile
    /// document (BUSINESS_REQUIREMENTS.md §17.3).</summary>
    public string? CertificateStorageKey { get; set; }
    public string? CertificateOriginalFileName { get; set; }
    public string? CertificateContentType { get; set; }

    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;
}

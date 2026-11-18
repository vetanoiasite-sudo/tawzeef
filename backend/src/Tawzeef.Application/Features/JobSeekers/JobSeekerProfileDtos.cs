using System.ComponentModel.DataAnnotations;

namespace Tawzeef.Application.Features.JobSeekers;

public class JobSeekerDocumentDto
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ReviewStatus { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class JobSeekerProfileDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Education { get; set; }
    public byte? YearsOfExperience { get; set; }
    public string? Skills { get; set; }
    public string? CareerSummary { get; set; }
    public string ReviewStatus { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public List<JobSeekerDocumentDto> Documents { get; set; } = [];
}

/// <summary>Only the "complete profile" fields (BUSINESS_REQUIREMENTS.md §5.1 Step 2–3) —
/// FullName/MobileNumber come from registration and change there, not here.</summary>
public class UpdateJobSeekerProfileDto
{
    [MaxLength(200), EmailAddress]
    public string? Email { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [MaxLength(200)]
    public string? Education { get; set; }

    public byte? YearsOfExperience { get; set; }

    [MaxLength(1000)]
    public string? Skills { get; set; }

    [MaxLength(2000)]
    public string? CareerSummary { get; set; }
}

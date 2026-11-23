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

    /// <summary>Only ever populated on the response to uploading a Cv document, and only when the
    /// local text-heuristic parser (ICvParser) found something — never written to the profile by
    /// the upload itself, purely a suggestion the job seeker reviews/edits in the profile form
    /// before saving. Never a phone number or email, by construction (CvParser excludes any line
    /// matching either pattern from every heuristic).</summary>
    public string? SuggestedJobTitle { get; set; }
    public byte? SuggestedYearsOfExperience { get; set; }
}

/// <summary>Mirrors JobApplications/JobApplicationDtos.cs's CvFileDto — same "stream straight
/// through IFileStorage, no signed URL" pattern (RBAC_PERMISSIONS.md §4.4/§5).</summary>
public class JobSeekerDocumentFileDto
{
    public Stream Content { get; set; } = Stream.Null;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
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
    public string? JobTitle { get; set; }

    /// <summary>Whether a profile photo is set — never a raw URL (the photo is private, not
    /// public like a company logo); the frontend fetches it through the authenticated
    /// GET /api/jobseekers/me/photo (or the office's GET /api/office/job-seekers/{id}/photo).</summary>
    public bool HasPhoto { get; set; }

    public string ReviewStatus { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public string? SuspensionReason { get; set; }
    public DateTime? SuspendedAt { get; set; }

    /// <summary>WORKFLOWS_AND_STATE_MACHINES.md §10.3 — count of complaints against this profile
    /// the office has confirmed Valid (RBAC_PERMISSIONS.md §0 item 5: only Valid ones count,
    /// cumulative, no time window). Office-review visibility only: always 0 on the self-service
    /// GET /api/jobseekers/me response, which reuses this same DTO.</summary>
    public int ValidComplaintCount { get; set; }

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

    [MaxLength(150)]
    public string? JobTitle { get; set; }
}

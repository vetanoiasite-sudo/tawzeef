using System.ComponentModel.DataAnnotations;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.JobApplications;

public class CreateJobApplicationDto
{
    [Required]
    public Guid JobAdvertisementId { get; set; }
}

public class JobApplicationIdDto
{
    public Guid Id { get; set; }
}

public class JobApplicationQueryDto
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; } = 20;
}

/// <summary>The Job Seeker's own "my applications" list — API_DOCUMENTATION.md §3.8.</summary>
public class MyApplicationListItemDto
{
    public Guid Id { get; set; }
    public Guid JobAdvertisementId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string EmployerDisplayName { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
    public string JobSeekerFacingStatus { get; set; } = string.Empty;
    public DateTime? WithdrawnAt { get; set; }
}

/// <summary>The Employer/office applicant list for one advertisement. Phone/Email/CvDownloadUrl
/// are omitted entirely (not nulled, not present) until RBAC_PERMISSIONS.md §5's payment gate
/// passes — office callers always get them (never payment-gated).</summary>
public class ApplicantListItemDto
{
    public Guid ApplicationId { get; set; }
    public string JobSeekerName { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
    public string EmployerFacingStatus { get; set; } = string.Empty;
    public DateTime? WithdrawnAt { get; set; }

    public string? Phone { get; set; }
    public string? Email { get; set; }

    /// <summary>A relative, authenticated API path (GET /api/employer/applications/{id}/cv) —
    /// not a signed query-string token, since no such file-serving infrastructure exists yet in
    /// this codebase. Present only once the gate passes, same as Phone/Email.</summary>
    public string? CvDownloadUrl { get; set; }
}

public class UpdateApplicationOutcomeDto
{
    [Required]
    public EmployerApplicationOutcome Outcome { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}

public class CvFileDto
{
    public Stream Content { get; set; } = Stream.Null;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}

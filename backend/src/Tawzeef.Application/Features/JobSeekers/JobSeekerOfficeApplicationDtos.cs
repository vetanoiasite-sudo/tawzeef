namespace Tawzeef.Application.Features.JobSeekers;

/// <summary>One row per office the platform has, for the job seeker's own "My Offices" view —
/// confirmed 2026-11-24: a job seeker may submit to several offices (or all of them), and each
/// reviews the same shared JobSeekerDocument set independently. "NotSubmitted" is a pseudo-status
/// (never stored) meaning no JobSeekerOfficeApplication row exists yet for that office.</summary>
public class JobSeekerOfficeStatusDto
{
    public Guid OfficeId { get; set; }
    public string OfficeName { get; set; } = string.Empty;
    public string? City { get; set; }

    public string ReviewStatus { get; set; } = "NotSubmitted";
    public string? RejectionReason { get; set; }
    public DateTime? SuspendedAt { get; set; }
    public string? SuspensionReason { get; set; }
    public DateTime? EmployedAt { get; set; }
    public DateTime? LeftJobAt { get; set; }
}

/// <summary>Submit to specific offices, or every currently-active office — used both right after
/// registration (if the job seeker skipped some) and any time later to add more.</summary>
public class SubmitJobSeekerOfficesDto
{
    public List<Guid> OfficeIds { get; set; } = [];
    public bool AllOffices { get; set; }
}

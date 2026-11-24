namespace Tawzeef.Application.Features.Employers;

/// <summary>One row per office the platform has, for the employer's own "My Offices" view —
/// confirmed 2026-11-24: an employer may submit to several offices (or all of them), and each
/// reviews the same shared EmployerDocument set independently. "NotSubmitted" is a pseudo-status
/// (never stored) meaning no EmployerOfficeApplication row exists yet for that office.</summary>
public class EmployerOfficeStatusDto
{
    public Guid OfficeId { get; set; }
    public string OfficeName { get; set; } = string.Empty;
    public string? City { get; set; }

    public string VerificationStatus { get; set; } = "NotSubmitted";
    public string? RejectionReason { get; set; }
    public string CooperationStatus { get; set; } = "Active";
    public string? CooperationStoppedReason { get; set; }
    public DateTime? CooperationStoppedAt { get; set; }
}

/// <summary>Submit to specific offices, or every currently-active office — the employer's old
/// one-time SelectOfficeAsync, now repeatable (registration itself still collects none, per
/// RegisterEmployerDto's doc comment — this is always a post-registration action).</summary>
public class SubmitEmployerOfficesDto
{
    public List<Guid> OfficeIds { get; set; } = [];
    public bool AllOffices { get; set; }
}

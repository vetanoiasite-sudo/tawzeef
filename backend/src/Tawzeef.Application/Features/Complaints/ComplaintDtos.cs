using System.ComponentModel.DataAnnotations;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.Complaints;

/// <summary>BUSINESS_REQUIREMENTS.md §11.2 / API_DOCUMENTATION.md §3.10. Scope note: this slice
/// covers submit -> office triage (validity decision) -> resolve, and a manual decide that fires
/// enforcement for a JobSeeker subject. The automatic 3-strike count/alert
/// (WORKFLOWS_AND_STATE_MACHINES.md §10.3) is deliberately not built yet — deferred until the
/// product owner confirms the open §11.3/§11.4 questions (does an Invalid complaint break the
/// count, is there a time window, etc.).</summary>
public class CreateComplaintDto
{
    [Required]
    public ComplaintSubjectType SubjectType { get; set; }

    [Required]
    public Guid SubjectId { get; set; }

    [Required, MaxLength(2000)]
    public string ComplaintText { get; set; } = string.Empty;
}

public class ComplaintDto
{
    public Guid Id { get; set; }
    public string SenderType { get; set; } = string.Empty;
    public string SubjectType { get; set; } = string.Empty;
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string ComplaintText { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ValidityDecision { get; set; } = string.Empty;
    public string? ResolutionNotes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ComplaintQueryDto
{
    public ComplaintStatus? Status { get; set; }
    public ComplaintValidityDecision? ValidityDecision { get; set; }
    public ComplaintSubjectType? SubjectType { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; } = 20;
}

/// <summary>Matches API_DOCUMENTATION.md §3.10's example exactly — triage is only ever about the
/// validity call, not the New/UnderReview/WaitingForInformation status itself (that moves
/// implicitly, the same way a JobSeeker profile does on GetById).</summary>
public class TriageComplaintDto
{
    [Required]
    public ComplaintValidityDecision ValidityDecision { get; set; }
}

public class ResolveComplaintDto
{
    /// <summary>Must be Resolved or Rejected — validated in the service, not here, since "any
    /// status" isn't a valid outcome of resolving.</summary>
    [Required]
    public ComplaintStatus Outcome { get; set; }

    [Required, MaxLength(2000)]
    public string ResolutionNotes { get; set; } = string.Empty;
}

public class DecideEnforcementDto
{
    [Required]
    public OfficeDecisionType DecisionType { get; set; }

    [Required, MaxLength(2000)]
    public string Reason { get; set; } = string.Empty;
}

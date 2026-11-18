namespace Tawzeef.Domain.Enums;

/// <summary>WORKFLOWS_AND_STATE_MACHINES.md §3 — structurally identical to JobSeekerReviewStatus.</summary>
public enum EmployerVerificationStatus : byte
{
    Draft = 0,
    Submitted = 1,
    UnderReview = 2,
    Approved = 3,
    Rejected = 4,
    Suspended = 5,
}

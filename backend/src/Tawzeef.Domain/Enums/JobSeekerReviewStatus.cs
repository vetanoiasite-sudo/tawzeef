namespace Tawzeef.Domain.Enums;

/// <summary>WORKFLOWS_AND_STATE_MACHINES.md §1.</summary>
public enum JobSeekerReviewStatus : byte
{
    Draft = 0,
    Submitted = 1,
    UnderReview = 2,
    Approved = 3,
    Rejected = 4,
    Suspended = 5,
}

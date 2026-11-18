namespace Tawzeef.Domain.Enums;

/// <summary>WORKFLOWS_AND_STATE_MACHINES.md §2 — shared by JobSeekerDocument and EmployerDocument.</summary>
public enum DocumentReviewStatus : byte
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
}

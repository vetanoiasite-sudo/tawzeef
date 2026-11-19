namespace Tawzeef.Domain.Enums;

/// <summary>WORKFLOWS_AND_STATE_MACHINES.md §10.1. WaitingForInformation and Closed are modeled
/// but not yet reachable through any endpoint in this slice — the former needs a staff/sender
/// messaging exchange that doesn't exist, the latter has no documented trigger separate from
/// Resolved/Rejected (API_DOCUMENTATION.md §3.10 lists no "close" action).</summary>
public enum ComplaintStatus : byte
{
    New = 0,
    UnderReview = 1,
    WaitingForInformation = 2,
    Resolved = 3,
    Rejected = 4,
    Closed = 5,
}

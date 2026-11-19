namespace Tawzeef.Domain.Enums;

/// <summary>WORKFLOWS_AND_STATE_MACHINES.md §9.1. Overdue has no background job to flip it —
/// computed at read time on the office due-list, same convention as JobAdvertisement expiry.</summary>
public enum FollowUpReportStatus : byte
{
    PendingDue = 0,
    Completed = 1,
    Overdue = 2,
}

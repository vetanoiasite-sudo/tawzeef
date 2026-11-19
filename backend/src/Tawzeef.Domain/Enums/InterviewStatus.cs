namespace Tawzeef.Domain.Enums;

/// <summary>WORKFLOWS_AND_STATE_MACHINES.md §8.</summary>
public enum InterviewStatus : byte
{
    Scheduled = 0,
    Completed = 1,
    Cancelled = 2,
    NoShow = 3,
}

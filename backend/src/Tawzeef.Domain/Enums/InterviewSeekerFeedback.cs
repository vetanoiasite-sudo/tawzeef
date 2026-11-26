namespace Tawzeef.Domain.Enums;

/// <summary>The job seeker's own self-reported account of how an interview went, collected a day
/// after Interview.ScheduledAt passes — independent of the employer-written InterviewReport.</summary>
public enum InterviewSeekerFeedback : byte
{
    Pending = 0,
    StillWaiting = 1,
    Accepted = 2,
    Rejected = 3,
}

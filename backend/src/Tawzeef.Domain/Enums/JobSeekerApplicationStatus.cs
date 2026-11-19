namespace Tawzeef.Domain.Enums;

/// <summary>WORKFLOWS_AND_STATE_MACHINES.md §7 — Job-seeker-facing track, recomputed from
/// EmployerApplicationOutcome by the service, never written directly by the Job Seeker.</summary>
public enum JobSeekerApplicationStatus : byte
{
    NotContacted = 0,
    ContactedNotSuitable = 1,
    ContactedInterviewScheduled = 2,
}

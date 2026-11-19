namespace Tawzeef.Domain.Enums;

/// <summary>WORKFLOWS_AND_STATE_MACHINES.md §7 — Employer-facing track of JobApplication.</summary>
public enum EmployerApplicationOutcome : byte
{
    NotYetContacted = 0,
    ContactedInterviewScheduled = 1,
    ContactedRefused = 2,
    NoResponseAfterContact = 3,
}

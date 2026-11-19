namespace Tawzeef.Domain.Enums;

/// <summary>DATABASE_DOCUMENTATION.md §6 — who changed a JobApplication status.</summary>
public enum ApplicationActorRole : byte
{
    Office = 0,
    Employer = 1,
    JobSeeker = 2,
}

namespace Tawzeef.Domain.Enums;

/// <summary>DATABASE_DOCUMENTATION.md §6 — discriminates which of JobApplication's two status
/// columns an ApplicationStatusHistory row recorded a change for.</summary>
public enum ApplicationStatusDomain : byte
{
    EmployerFacing = 0,
    JobSeekerFacing = 1,
}

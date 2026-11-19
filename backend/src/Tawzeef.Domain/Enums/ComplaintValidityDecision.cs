namespace Tawzeef.Domain.Enums;

/// <summary>RBAC_PERMISSIONS.md §0 item 5: only a Valid complaint ever counts toward the 3-strike
/// threshold — Pending and Invalid never do.</summary>
public enum ComplaintValidityDecision : byte
{
    Pending = 0,
    Valid = 1,
    Invalid = 2,
}

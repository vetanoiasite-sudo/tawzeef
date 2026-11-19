namespace Tawzeef.Domain.Enums;

/// <summary>WORKFLOWS_AND_STATE_MACHINES.md §6. Refunded is modeled but unused in v1 (RBAC_PERMISSIONS.md §0 item 20).</summary>
public enum PaymentStatus : byte
{
    Pending = 0,
    Paid = 1,
    Failed = 2,
    Refunded = 3,
}

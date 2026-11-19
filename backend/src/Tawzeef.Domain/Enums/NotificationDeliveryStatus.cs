namespace Tawzeef.Domain.Enums;

/// <summary>DATABASE_DOCUMENTATION.md §7 — confirmed in-app only for v1 (RBAC_PERMISSIONS.md §0
/// item 22), so this column stays nullable/unused rather than being removed, for a future
/// SMS/email channel to use without a migration.</summary>
public enum NotificationDeliveryStatus : byte
{
    Pending = 0,
    Sent = 1,
    Failed = 2,
}

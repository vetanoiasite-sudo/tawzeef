using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.Notifications;

/// <summary>DATABASE_DOCUMENTATION.md §7. Append-only in practice except for ReadAt — deliberately
/// not an IEntity, same convention as OfficeDecision: no soft-delete/modification tracking, its own
/// CreatedAt stamp. EventType is a plain string (not an enum) so a new event type never needs a
/// migration, per the doc's own stated design intent.</summary>
public class Notification
{
    public Guid Id { get; set; }

    /// <summary>Nullable only for a platform-level notification to a Super Admin.</summary>
    public Guid? OfficeId { get; set; }
    public Guid RecipientUserId { get; set; }

    public string EventType { get; set; } = string.Empty;
    public string? RelatedType { get; set; }
    public Guid? RelatedId { get; set; }
    public string Message { get; set; } = string.Empty;

    public DateTime? ReadAt { get; set; }

    /// <summary>Confirmed unused in v1 (in-app only) — kept nullable for a future external channel.</summary>
    public NotificationDeliveryStatus? DeliveryStatus { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

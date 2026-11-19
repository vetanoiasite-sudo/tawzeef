using Tawzeef.Domain.Entities.Notifications;

namespace Tawzeef.Application.Features.Notifications;

internal static class NotificationMapping
{
    public static NotificationDto ToDto(this Notification n) => new()
    {
        Id = n.Id,
        EventType = n.EventType,
        RelatedType = n.RelatedType,
        RelatedId = n.RelatedId,
        Message = n.Message,
        ReadAt = n.ReadAt,
        CreatedAt = n.CreatedAt,
    };
}

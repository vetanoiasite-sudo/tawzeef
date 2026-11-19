namespace Tawzeef.Application.Features.Notifications;

public class NotificationDto
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? RelatedType { get; set; }
    public Guid? RelatedId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NotificationQueryDto
{
    public bool UnreadOnly { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; } = 20;
}

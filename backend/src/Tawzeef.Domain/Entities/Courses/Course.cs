using Tawzeef.Domain.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.Courses;

/// <summary>Office-authored, office-published, no review workflow — the office is its own
/// gatekeeper for its own courses, same reasoning as its banners. Deliberately no price field
/// (explicit product decision, 2026-11-24): this is a promotional listing, not a store.</summary>
public class Course : IEntity
{
    public Guid Id { get; set; }
    public Guid OfficeId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ContactPhone { get; set; } = string.Empty;

    public string? ImageStorageKey { get; set; }
    public string? ImageOriginalFileName { get; set; }
    public string? ImageContentType { get; set; }

    /// <summary>Active = visible on the public /courses listing; Paused = hidden entirely, even
    /// from a direct link (same as JobAdvertisement.Paused), but never deleted.</summary>
    public PublishStatus Status { get; set; } = PublishStatus.Active;

    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;
}

using Tawzeef.Domain.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.Courses;

/// <summary>A home-page promotional banner — image only, no link (explicit product decision,
/// 2026-11-24). Office-authored and office-published, same gatekeeping model as Course.</summary>
public class OfficeBanner : IEntity
{
    public Guid Id { get; set; }
    public Guid OfficeId { get; set; }

    public string ImageStorageKey { get; set; } = string.Empty;
    public string ImageOriginalFileName { get; set; } = string.Empty;
    public string ImageContentType { get; set; } = string.Empty;

    public PublishStatus Status { get; set; } = PublishStatus.Active;

    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;
}

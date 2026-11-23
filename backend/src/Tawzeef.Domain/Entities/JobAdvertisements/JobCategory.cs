using Tawzeef.Domain.Common;

namespace Tawzeef.Domain.Entities.JobAdvertisements;

/// <summary>DATABASE_DOCUMENTATION.md §7.1's proposed JobCategory lookup table, now confirmed and
/// built — office-controlled only. An employer's free-text <see cref="JobAdvertisement.Category"/>
/// stays exactly as it was (their own suggestion), but it's never the source of truth for search
/// or display once an ad has a real <see cref="JobAdvertisement.CategoryId"/>: the office assigns
/// that during review, via this lookup — either picking an existing row or creating a new one
/// inline (JobAdvertisementReviewService.AssignCategoryAsync), never the employer.</summary>
public class JobCategory : IEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;
}

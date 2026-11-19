using Tawzeef.Domain.Entities.JobSeekers;

namespace Tawzeef.Application.Features.JobSeekers;

internal static class JobSeekerReviewMapping
{
    public static JobSeekerReviewListItemDto ToListItemDto(this JobSeekerProfile p) => new()
    {
        Id = p.Id,
        FullName = p.FullName,
        MobileNumber = p.MobileNumber,
        City = p.City,
        ReviewStatus = p.ReviewStatus.ToString(),
        LastUpdatedAt = p.ModifiedDate ?? p.CreationDate,
    };
}

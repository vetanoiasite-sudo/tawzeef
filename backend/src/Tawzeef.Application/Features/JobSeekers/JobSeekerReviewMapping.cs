using Tawzeef.Domain.Entities.JobSeekers;

namespace Tawzeef.Application.Features.JobSeekers;

internal static class JobSeekerReviewMapping
{
    public static JobSeekerReviewListItemDto ToListItemDto(this JobSeekerProfile p, JobSeekerOfficeApplication application, int validComplaintCount = 0) => new()
    {
        Id = p.Id,
        FullName = p.FullName,
        MobileNumber = p.MobileNumber,
        City = p.City,
        ReviewStatus = application.ReviewStatus.ToString(),
        LastUpdatedAt = application.ModifiedDate ?? application.CreationDate,
        ValidComplaintCount = validComplaintCount,
    };
}

using Tawzeef.Domain.Entities.Employers;

namespace Tawzeef.Application.Features.Employers;

internal static class EmployerReviewMapping
{
    public static EmployerReviewListItemDto ToListItemDto(this Employer e, EmployerOfficeApplication application, int validComplaintCount = 0) => new()
    {
        Id = e.Id,
        LegalName = e.LegalName,
        DisplayName = e.DisplayName,
        City = e.City,
        VerificationStatus = application.VerificationStatus.ToString(),
        CooperationStatus = application.CooperationStatus.ToString(),
        LastUpdatedAt = application.ModifiedDate ?? application.CreationDate,
        ValidComplaintCount = validComplaintCount,
    };
}

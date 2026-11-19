using Tawzeef.Domain.Entities.Employers;

namespace Tawzeef.Application.Features.Employers;

internal static class EmployerReviewMapping
{
    public static EmployerReviewListItemDto ToListItemDto(this Employer e, int validComplaintCount = 0) => new()
    {
        Id = e.Id,
        LegalName = e.LegalName,
        DisplayName = e.DisplayName,
        City = e.City,
        VerificationStatus = e.VerificationStatus.ToString(),
        CooperationStatus = e.CooperationStatus.ToString(),
        LastUpdatedAt = e.ModifiedDate ?? e.CreationDate,
        ValidComplaintCount = validComplaintCount,
    };
}

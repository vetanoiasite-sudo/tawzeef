using Tawzeef.Domain.Entities.Employers;

namespace Tawzeef.Application.Features.Employers;

internal static class EmployerProfileMapping
{
    public static EmployerProfileDto ToDto(this Employer e, IEnumerable<EmployerDocument> documents, int validComplaintCount = 0) => new()
    {
        Id = e.Id,
        LegalName = e.LegalName,
        DisplayName = e.DisplayName,
        BusinessActivity = e.BusinessActivity,
        Address = e.Address,
        City = e.City,
        CompanyEmail = e.CompanyEmail,
        CompanyPhone = e.CompanyPhone,
        RepresentativeName = e.RepresentativeName,
        RepresentativeRole = e.RepresentativeRole,
        VerificationStatus = e.VerificationStatus.ToString(),
        RejectionReason = e.RejectionReason,
        CooperationStatus = e.CooperationStatus.ToString(),
        CooperationStoppedReason = e.CooperationStoppedReason,
        CooperationStoppedAt = e.CooperationStoppedAt,
        ValidComplaintCount = validComplaintCount,
        Documents = documents.Select(d => d.ToDto()).ToList(),
    };

    public static EmployerDocumentDto ToDto(this EmployerDocument d) => new()
    {
        Id = d.Id,
        DocumentType = d.DocumentType.ToString(),
        OriginalFileName = d.OriginalFileName,
        ReviewStatus = d.ReviewStatus.ToString(),
        RejectionReason = d.RejectionReason,
        UploadedAt = d.UploadedAt,
    };
}

using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Entities.Offices;

namespace Tawzeef.Application.Features.Employers;

internal static class EmployerProfileMapping
{
    public static EmployerProfileDto ToDto(
        this Employer e,
        IEnumerable<EmployerDocument> documents,
        int validComplaintCount = 0,
        EmployerOfficeApplication? officeApplication = null,
        EmploymentOffice? office = null) => new()
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
        LogoUrl = e.LogoStorageKey is null ? null : $"/api/employers/{e.Id}/logo",
        ValidComplaintCount = validComplaintCount,
        OfficeApplication = officeApplication?.ToDto(office),
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

    public static EmployerOfficeStatusDto ToDto(this EmployerOfficeApplication a, EmploymentOffice? office = null) => new()
    {
        OfficeId = a.OfficeId,
        OfficeName = office?.ArName ?? office?.Name ?? string.Empty,
        City = office?.City,
        VerificationStatus = a.VerificationStatus.ToString(),
        RejectionReason = a.RejectionReason,
        CooperationStatus = a.CooperationStatus.ToString(),
        CooperationStoppedReason = a.CooperationStoppedReason,
        CooperationStoppedAt = a.CooperationStoppedAt,
    };
}

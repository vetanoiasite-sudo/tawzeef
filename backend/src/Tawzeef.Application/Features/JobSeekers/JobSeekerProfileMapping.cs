using Tawzeef.Domain.Entities.JobSeekers;
using Tawzeef.Domain.Entities.Offices;

namespace Tawzeef.Application.Features.JobSeekers;

internal static class JobSeekerProfileMapping
{
    public static JobSeekerProfileDto ToDto(
        this JobSeekerProfile p,
        IEnumerable<JobSeekerDocument> documents,
        int validComplaintCount = 0,
        JobSeekerOfficeApplication? officeApplication = null,
        EmploymentOffice? office = null) => new()
    {
        Id = p.Id,
        FullName = p.FullName,
        NationalId = p.NationalId,
        MobileNumber = p.MobileNumber,
        Email = p.Email,
        Address = p.Address,
        City = p.City,
        DateOfBirth = p.DateOfBirth,
        Education = p.Education,
        YearsOfExperience = p.YearsOfExperience,
        Skills = p.Skills,
        CareerSummary = p.CareerSummary,
        JobTitle = p.JobTitle,
        DesiredCategoryId = p.DesiredCategoryId,
        HasPhoto = p.PhotoStorageKey != null,
        ValidComplaintCount = validComplaintCount,
        OfficeApplication = officeApplication?.ToDto(office),
        Documents = documents.Select(d => d.ToDto()).ToList(),
    };

    public static JobSeekerDocumentDto ToDto(this JobSeekerDocument d) => new()
    {
        Id = d.Id,
        DocumentType = d.DocumentType.ToString(),
        OriginalFileName = d.OriginalFileName,
        ReviewStatus = d.ReviewStatus.ToString(),
        RejectionReason = d.RejectionReason,
        UploadedAt = d.UploadedAt,
    };

    public static JobSeekerOfficeStatusDto ToDto(this JobSeekerOfficeApplication a, EmploymentOffice? office = null) => new()
    {
        OfficeId = a.OfficeId,
        OfficeName = office?.ArName ?? office?.Name ?? string.Empty,
        City = office?.City,
        ReviewStatus = a.ReviewStatus.ToString(),
        RejectionReason = a.RejectionReason,
        SuspendedAt = a.SuspendedAt,
        SuspensionReason = a.SuspensionReason,
        EmployedAt = a.EmployedAt,
        LeftJobAt = a.LeftJobAt,
    };
}

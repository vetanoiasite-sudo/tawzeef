using Tawzeef.Domain.Entities.JobSeekers;

namespace Tawzeef.Application.Features.JobSeekers;

internal static class JobSeekerProfileMapping
{
    public static JobSeekerProfileDto ToDto(this JobSeekerProfile p, IEnumerable<JobSeekerDocument> documents, int validComplaintCount = 0) => new()
    {
        Id = p.Id,
        FullName = p.FullName,
        MobileNumber = p.MobileNumber,
        Email = p.Email,
        Address = p.Address,
        City = p.City,
        DateOfBirth = p.DateOfBirth,
        Education = p.Education,
        YearsOfExperience = p.YearsOfExperience,
        Skills = p.Skills,
        CareerSummary = p.CareerSummary,
        ReviewStatus = p.ReviewStatus.ToString(),
        RejectionReason = p.RejectionReason,
        SuspensionReason = p.SuspensionReason,
        SuspendedAt = p.SuspendedAt,
        ValidComplaintCount = validComplaintCount,
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
}

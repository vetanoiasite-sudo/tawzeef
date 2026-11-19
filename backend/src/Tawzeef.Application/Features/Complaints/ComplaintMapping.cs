using Tawzeef.Domain.Entities.Complaints;

namespace Tawzeef.Application.Features.Complaints;

internal static class ComplaintMapping
{
    /// <summary>subjectName is resolved by the caller (a JobSeekerProfile.FullName or
    /// Employer.DisplayName lookup) — kept out of this pure mapper since it needs a DB round trip.</summary>
    public static ComplaintDto ToDto(this Complaint c, string subjectName) => new()
    {
        Id = c.Id,
        SenderType = c.SenderType.ToString(),
        SubjectType = c.SubjectType.ToString(),
        SubjectId = c.SubjectId,
        SubjectName = subjectName,
        ComplaintText = c.ComplaintText,
        Status = c.Status.ToString(),
        ValidityDecision = c.ValidityDecision.ToString(),
        ResolutionNotes = c.ResolutionNotes,
        CreatedAt = c.CreationDate,
    };
}

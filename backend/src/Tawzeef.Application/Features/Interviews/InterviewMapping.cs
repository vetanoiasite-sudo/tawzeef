using Tawzeef.Domain.Entities.Interviews;

namespace Tawzeef.Application.Features.Interviews;

internal static class InterviewMapping
{
    public static InterviewDto ToDto(this Interview i) => new()
    {
        Id = i.Id,
        ScheduledAt = i.ScheduledAt,
        LocationOrMeetingDetails = i.LocationOrMeetingDetails,
        Status = i.Status.ToString(),
    };

    public static InterviewReportDto ToDto(this InterviewReport r) => new()
    {
        Id = r.Id,
        Attendance = r.Attendance.ToString(),
        InterviewResult = r.InterviewResult,
        EmployerEvaluation = r.EmployerEvaluation,
        Strengths = r.Strengths,
        Concerns = r.Concerns,
        Recommendation = r.Recommendation,
        Notes = r.Notes,
        CreationDate = r.CreationDate,
    };
}

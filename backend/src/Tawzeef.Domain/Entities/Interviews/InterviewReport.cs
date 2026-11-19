using Tawzeef.Domain.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.Interviews;

/// <summary>DATABASE_DOCUMENTATION.md §6 — the "first report" (BUSINESS_REQUIREMENTS.md §10.1).
/// Confirmed written by the Employer. One per JobApplication — a unique index, same convention as
/// ApplicantEvaluationReport, since "the first report" is singular by framing.</summary>
public class InterviewReport : IEntity
{
    public Guid Id { get; set; }

    public Guid OfficeId { get; set; }
    public Guid JobApplicationId { get; set; }
    public Guid? InterviewId { get; set; }

    public AttendanceResult Attendance { get; set; }
    public string? InterviewResult { get; set; }
    public string? EmployerEvaluation { get; set; }
    public string? Strengths { get; set; }
    public string? Concerns { get; set; }
    public string? Recommendation { get; set; }
    public string? Notes { get; set; }

    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;
}

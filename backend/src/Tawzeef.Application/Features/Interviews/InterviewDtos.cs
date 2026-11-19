using System.ComponentModel.DataAnnotations;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.Interviews;

public class CreateInterviewDto
{
    public DateTime? ScheduledAt { get; set; }

    [MaxLength(500)]
    public string? LocationOrMeetingDetails { get; set; }
}

public class UpdateInterviewDto
{
    public DateTime? ScheduledAt { get; set; }

    [MaxLength(500)]
    public string? LocationOrMeetingDetails { get; set; }

    /// <summary>Null keeps the current status (a plain reschedule); otherwise must be one of the
    /// three terminal values — WORKFLOWS_AND_STATE_MACHINES.md §8.</summary>
    public InterviewStatus? Status { get; set; }
}

public class InterviewDto
{
    public Guid Id { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public string? LocationOrMeetingDetails { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateInterviewReportDto
{
    [Required]
    public AttendanceResult Attendance { get; set; }

    [MaxLength(100)]
    public string? InterviewResult { get; set; }

    [MaxLength(2000)]
    public string? EmployerEvaluation { get; set; }

    [MaxLength(1000)]
    public string? Strengths { get; set; }

    [MaxLength(1000)]
    public string? Concerns { get; set; }

    [MaxLength(1000)]
    public string? Recommendation { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}

public class InterviewReportDto
{
    public Guid Id { get; set; }
    public string Attendance { get; set; } = string.Empty;
    public string? InterviewResult { get; set; }
    public string? EmployerEvaluation { get; set; }
    public string? Strengths { get; set; }
    public string? Concerns { get; set; }
    public string? Recommendation { get; set; }
    public string? Notes { get; set; }
    public DateTime CreationDate { get; set; }
}

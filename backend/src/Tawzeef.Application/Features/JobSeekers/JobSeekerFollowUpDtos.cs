using System.ComponentModel.DataAnnotations;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.JobSeekers;

public class RespondEmploymentCheckDto
{
    [Required]
    public bool FoundJob { get; set; }
}

public class RespondHiringFollowUpDto
{
    [Required]
    public bool StillEmployed { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}

public class RespondInterviewFeedbackDto
{
    [Required]
    public InterviewSeekerFeedback Outcome { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}

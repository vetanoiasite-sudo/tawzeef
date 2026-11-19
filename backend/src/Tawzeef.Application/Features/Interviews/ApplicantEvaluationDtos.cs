using System.ComponentModel.DataAnnotations;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.Interviews;

public class CreateApplicantEvaluationDto
{
    [Required]
    public ApplicantEvaluationOutcome Outcome { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}

public class ApplicantEvaluationDto
{
    public Guid Id { get; set; }
    public string Outcome { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime SubmittedAt { get; set; }
}

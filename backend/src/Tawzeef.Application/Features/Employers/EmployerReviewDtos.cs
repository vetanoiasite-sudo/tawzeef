using System.ComponentModel.DataAnnotations;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.Employers;

/// <summary>The office-staff review queue — mirrors JobSeekerReviewDtos.cs exactly.</summary>
public class EmployerReviewQueryDto
{
    public EmployerVerificationStatus? VerificationStatus { get; set; }
    public string? DisplayName { get; set; }
    public string? City { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; } = 20;
}

public class EmployerReviewListItemDto
{
    public Guid Id { get; set; }
    public string LegalName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? City { get; set; }
    public string VerificationStatus { get; set; } = string.Empty;
    public string CooperationStatus { get; set; } = string.Empty;
    public DateTime LastUpdatedAt { get; set; }
    public int ValidComplaintCount { get; set; }
}

/// <summary>A required free-text reason — document/profile reject, stop-cooperation, and
/// resume-cooperation all just need "why", nothing else.</summary>
public class EmployerReasonDto
{
    [Required, MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;
}

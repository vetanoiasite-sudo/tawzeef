using System.ComponentModel.DataAnnotations;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.JobSeekers;

/// <summary>The office-staff review queue — RBAC_PERMISSIONS.md §3's plain
/// jobseekers.view/.review/.approve/.reject codes, not the self-service "...own" verbs in
/// JobSeekerProfileDtos.cs. Detail view reuses JobSeekerProfileDto — a staff reviewer needs the
/// same fields a job seeker sees of their own profile.</summary>
public class JobSeekerReviewQueryDto
{
    public JobSeekerReviewStatus? ReviewStatus { get; set; }
    public string? Name { get; set; }
    public string? City { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; } = 20;
}

public class JobSeekerReviewListItemDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? City { get; set; }
    public string ReviewStatus { get; set; } = string.Empty;
    public DateTime LastUpdatedAt { get; set; }
}

public class RejectReasonDto
{
    [Required, MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;
}

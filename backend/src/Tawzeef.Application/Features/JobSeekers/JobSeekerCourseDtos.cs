using System.ComponentModel.DataAnnotations;

namespace Tawzeef.Application.Features.JobSeekers;

public class CreateJobSeekerCourseDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Institution { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? WhatLearned { get; set; }
}

/// <summary>Full-replace PUT, same convention as every other profile-update DTO in this
/// codebase.</summary>
public class UpdateJobSeekerCourseDto : CreateJobSeekerCourseDto;

public class JobSeekerCourseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Institution { get; set; } = string.Empty;
    public string? WhatLearned { get; set; }
    public bool HasCertificate { get; set; }
    public string? CertificateFileName { get; set; }
    public DateTime CreationDate { get; set; }
}

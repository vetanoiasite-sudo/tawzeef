using System.ComponentModel.DataAnnotations;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.JobAdvertisements;

public class CreateJobAdvertisementDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? ExperienceRequirement { get; set; }

    [MaxLength(100)]
    public string? WorkingHours { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    public EmploymentType? EmploymentType { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public int VacancyCount { get; set; } = 1;
}

/// <summary>Full-replace PUT, same convention as every other profile-update DTO in this
/// codebase — the frontend form must pre-populate every field as defaultValues.</summary>
public class UpdateJobAdvertisementDto : CreateJobAdvertisementDto;

public class JobAdvertisementReasonDto
{
    [Required, MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;
}

public class ConfirmAdvertisementPaymentDto
{
    [Range(0.01, 1_000_000)]
    public decimal Amount { get; set; }

    [MaxLength(3)]
    public string Currency { get; set; } = "EGP";
}

public class AdvertisementPaymentDto
{
    public string PaymentStatus { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
}

public class JobAdvertisementListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Category { get; set; }
    public string Status { get; set; } = string.Empty;
    public int VacancyCount { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Only populated on the office review queue — the employer's own list already knows
    /// whose ads they are.</summary>
    public string? EmployerDisplayName { get; set; }
}

public class JobAdvertisementDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ExperienceRequirement { get; set; }
    public string? WorkingHours { get; set; }
    public string? Address { get; set; }
    public string? Category { get; set; }
    public string? City { get; set; }
    public string? EmploymentType { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public int VacancyCount { get; set; }

    public string Status { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public Guid? RenewedFromAdvertisementId { get; set; }

    public string? EmployerDisplayName { get; set; }
    public AdvertisementPaymentDto? Payment { get; set; }
}

public class JobAdvertisementQueryDto
{
    public JobAdvertisementStatus? Status { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; } = 20;
}

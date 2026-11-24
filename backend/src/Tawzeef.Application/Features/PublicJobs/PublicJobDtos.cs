using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.PublicJobs;

/// <summary>Anonymous search query — API_DOCUMENTATION.md §3.7. category/city are free-text
/// Contains filters, not lookup-table ids: DATABASE_DOCUMENTATION.md §7.1's JobCategory/City
/// tables are a proposed, not-yet-confirmed addition, so this follows the doc's own documented
/// fallback ("keep City/CategoryId as free text and build filters off a DISTINCT query").</summary>
public class JobSearchQueryDto
{
    public string? Keyword { get; set; }
    public string? City { get; set; }
    public string? Category { get; set; }
    public EmploymentType? EmploymentType { get; set; }
    /// <summary>Free-text Contains match on the employer's public display name — same convention
    /// as City/Category above (no lookup table, no exact id match: a job seeker browsing doesn't
    /// know an employer's id).</summary>
    public string? EmployerName { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; } = 20;
}

/// <summary>Never includes employer contact info (BUSINESS_REQUIREMENTS.md §6.2).</summary>
public class JobListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string EmployerDisplayName { get; set; } = string.Empty;
    public string? EmployerLogoUrl { get; set; }
    /// <summary>Confirmed 2026-11-24: every job ad must show which office it belongs to.</summary>
    public Guid OfficeId { get; set; }
    public string OfficeName { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Category { get; set; }
    public string? EmploymentType { get; set; }
    public string? ExperienceRequirement { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>Home page "trust strip" — real counts, no fabricated numbers. Anonymous, cheap
/// (three COUNT queries), never includes anything identifying.</summary>
public class PublicPlatformStatsDto
{
    public int ActiveJobsCount { get; set; }
    public int ApprovedEmployersCount { get; set; }
    public int ApprovedJobSeekersCount { get; set; }
}

/// <summary>A city actually carried by at least one live ad — DISTINCT-over-free-text, same
/// documented fallback as City/Category filtering (this file's own JobSearchQueryDto doc
/// comment), used for the home page "browse by city" section instead of a static list.</summary>
public class CityCountDto
{
    public string City { get; set; } = string.Empty;
    public int JobCount { get; set; }
}

/// <summary>No employerEmail/employerPhone field at all — not a null one — API_DOCUMENTATION.md
/// §3.7: there must be nothing for a bug or devtools session to expose.</summary>
public class JobDetailsDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ExperienceRequirement { get; set; }
    public string? WorkingHours { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Category { get; set; }
    public string? EmploymentType { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    // No VacancyCount — never shown to a job seeker (only the employer and the office see it),
    // per explicit product decision. The employer/office DTOs in JobAdvertisementDtos.cs still
    // carry it.
    public string EmployerDisplayName { get; set; } = string.Empty;
    public string? EmployerLogoUrl { get; set; }
    /// <summary>Confirmed 2026-11-24: every job ad must show which office it belongs to.</summary>
    public Guid OfficeId { get; set; }
    public string OfficeName { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string ContactNote { get; set; } = "التواصل يتم من خلال مكتب التوظيف فقط.";
}

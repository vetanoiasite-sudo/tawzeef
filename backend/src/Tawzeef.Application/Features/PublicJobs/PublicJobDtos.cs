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
    public int PageIndex { get; set; }
    public int PageSize { get; set; } = 20;
}

/// <summary>Never includes employer contact info (BUSINESS_REQUIREMENTS.md §6.2).</summary>
public class JobListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string EmployerDisplayName { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Category { get; set; }
    public string? EmploymentType { get; set; }
    public string? ExperienceRequirement { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
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
    public int VacancyCount { get; set; }
    public string EmployerDisplayName { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string ContactNote { get; set; } = "التواصل يتم من خلال مكتب التوظيف فقط.";
}

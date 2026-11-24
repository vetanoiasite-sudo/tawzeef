using Tawzeef.Domain.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.Employers;

/// <summary>DATABASE_DOCUMENTATION.md §5. Created as a Draft stub at registration; fields beyond
/// LegalName/DisplayName/RepresentativeName are filled in by the "complete company profile" step
/// (not built yet) — same deferral as JobSeekerProfile.</summary>
public class Employer : IEntity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string LegalName { get; set; } = string.Empty;
    /// <summary>Shown publicly on job listings — never contact info (BUSINESS_REQUIREMENTS.md §6.2).</summary>
    public string DisplayName { get; set; } = string.Empty;
    public string? BusinessActivity { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    /// <summary>Private — never serialized into a public job DTO.</summary>
    public string? CompanyEmail { get; set; }
    /// <summary>Private — same.</summary>
    public string? CompanyPhone { get; set; }
    public string? RepresentativeName { get; set; }
    public string? RepresentativeRole { get; set; }

    /// <summary>Company logo — unlike every other upload in this app, this is meant to be
    /// publicly visible on job listings, so it's served through an [AllowAnonymous] endpoint
    /// instead of the authenticated-only document download pattern. No review workflow: a new
    /// upload just replaces the previous one.</summary>
    public string? LogoStorageKey { get; set; }
    public string? LogoOriginalFileName { get; set; }
    public string? LogoContentType { get; set; }

    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;
}

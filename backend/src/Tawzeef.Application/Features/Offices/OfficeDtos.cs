using System.ComponentModel.DataAnnotations;

namespace Tawzeef.Application.Features.Offices;

public class CreateOfficeDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? ArName { get; set; }

    [Required, MaxLength(30)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(200), EmailAddress]
    public string? Email { get; set; }
}

public class OfficeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ArName { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Status { get; set; } = string.Empty;

    /// <summary>A relative API path (e.g. /api/offices/{id}/logo), never the raw storage key —
    /// null when no logo has been uploaded yet. The endpoint it points to is [AllowAnonymous]
    /// (same reasoning as EmployerProfileDto.LogoUrl).</summary>
    public string? LogoUrl { get; set; }
}

/// <summary>What the public registration dropdown gets — nothing beyond name/city/logo (API_DOCUMENTATION.md §3.2).</summary>
public class PublicOfficeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? LogoUrl { get; set; }
}

/// <summary>Mirrors Employers/EmployerProfileDtos.cs's EmployerDocumentFileDto — same "stream
/// straight through IFileStorage" pattern, reused here for an office's logo file.</summary>
public class OfficeLogoFileDto
{
    public Stream Content { get; set; } = Stream.Null;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}

/// <summary>Code is immutable once an office exists — it's the tenant key other records link to.</summary>
public class UpdateOfficeDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? ArName { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(200), EmailAddress]
    public string? Email { get; set; }
}

public class OfficePlatformStatsDto
{
    public int TotalOfficesCount { get; set; }
    public int ActiveOfficesCount { get; set; }
    public int InactiveOfficesCount { get; set; }
}

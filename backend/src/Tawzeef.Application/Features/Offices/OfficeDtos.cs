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
}

/// <summary>What the public registration dropdown gets — nothing beyond name/city (API_DOCUMENTATION.md §3.2).</summary>
public class PublicOfficeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
}

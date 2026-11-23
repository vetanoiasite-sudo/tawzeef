using System.ComponentModel.DataAnnotations;

namespace Tawzeef.Application.Features.Employers;

public class EmployerDocumentDto
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ReviewStatus { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime UploadedAt { get; set; }
}

/// <summary>Mirrors JobSeekers/JobSeekerProfileDtos.cs's JobSeekerDocumentFileDto — same "stream
/// straight through IFileStorage, no signed URL" pattern (RBAC_PERMISSIONS.md §4.4/§5).</summary>
public class EmployerDocumentFileDto
{
    public Stream Content { get; set; } = Stream.Null;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}

public class EmployerProfileDto
{
    public Guid Id { get; set; }
    public string LegalName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? BusinessActivity { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? CompanyEmail { get; set; }
    public string? CompanyPhone { get; set; }
    public string? RepresentativeName { get; set; }
    public string? RepresentativeRole { get; set; }

    /// <summary>A relative API path (e.g. /api/employers/{id}/logo), never the raw storage key —
    /// null when no logo has been uploaded yet. The endpoint it points to is [AllowAnonymous].</summary>
    public string? LogoUrl { get; set; }

    public string VerificationStatus { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }

    public string CooperationStatus { get; set; } = string.Empty;
    public string? CooperationStoppedReason { get; set; }
    public DateTime? CooperationStoppedAt { get; set; }

    /// <summary>Same idea as JobSeekerProfileDto.ValidComplaintCount — always 0 on the self-service
    /// GET /api/employers/me response, which reuses this same DTO.</summary>
    public int ValidComplaintCount { get; set; }

    public List<EmployerDocumentDto> Documents { get; set; } = [];
}

/// <summary>Only the "complete company profile" fields — LegalName/DisplayName come from
/// registration and change there, not here (same exclusion rule as JobSeeker's FullName/MobileNumber).</summary>
public class UpdateEmployerProfileDto
{
    [MaxLength(200)]
    public string? BusinessActivity { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(200), EmailAddress]
    public string? CompanyEmail { get; set; }

    [MaxLength(30)]
    public string? CompanyPhone { get; set; }

    [MaxLength(200)]
    public string? RepresentativeName { get; set; }

    [MaxLength(100)]
    public string? RepresentativeRole { get; set; }
}

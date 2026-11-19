using System.ComponentModel.DataAnnotations;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.Auth;

public class RequestOtpDto
{
    [Required, MaxLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;

    public OtpPurpose Purpose { get; set; } = OtpPurpose.Registration;
}

public class VerifyOtpDto
{
    [Required, MaxLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    public OtpPurpose Purpose { get; set; } = OtpPurpose.Registration;
}

public class OtpVerifiedDto
{
    public string OtpToken { get; set; } = string.Empty;
}

public class RegisterJobSeekerDto
{
    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string MobileNumber { get; set; } = string.Empty;

    [Required]
    public string OtpToken { get; set; } = string.Empty;

    [MaxLength(200), EmailAddress]
    public string? Email { get; set; }

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public Guid OfficeId { get; set; }
}

public class RegisterEmployerDto
{
    [Required, MaxLength(200)]
    public string RepresentativeName { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string MobileNumber { get; set; } = string.Empty;

    [Required]
    public string OtpToken { get; set; } = string.Empty;

    [MaxLength(200), EmailAddress]
    public string? Email { get; set; }

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public Guid OfficeId { get; set; }

    [Required, MaxLength(250)]
    public string LegalName { get; set; } = string.Empty;

    [Required, MaxLength(250)]
    public string DisplayName { get; set; } = string.Empty;
}

/// <summary>The invitee sets their own password here — the inviting OfficeAdmin/SuperAdmin never
/// sets or sees it (OfficeStaffInvite doc comment).</summary>
public class AcceptOfficeInviteDto
{
    [Required, MaxLength(30)]
    public string MobileNumber { get; set; } = string.Empty;

    [Required]
    public string OtpToken { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required]
    public string Identifier { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResultDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public Guid? JobSeekerProfileId { get; set; }
    public Guid? EmployerId { get; set; }

    /// <summary>"SuperAdmin" | "OfficeAdmin" | "OfficeStaff" | "JobSeeker" | "Employer" — API_DOCUMENTATION.md §3.1,
    /// what the frontend router uses to pick a landing page.</summary>
    public string Role { get; set; } = string.Empty;
    public Guid? OfficeId { get; set; }

    /// <summary>What useAccess().hasPermission() gates the office-staff UI on
    /// (PROJECT_ARCHITECTURE_FRONTEND.md §13.3) — empty for SuperAdmin/JobSeeker/Employer, who are
    /// each covered by their own shortcut on the frontend instead (RBAC_PERMISSIONS.md §2/§3).</summary>
    public List<string> PermissionCodes { get; set; } = [];

    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

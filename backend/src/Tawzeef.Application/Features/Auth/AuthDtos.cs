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

    /// <summary>"SuperAdmin" | "OfficeAdmin" | "OfficeStaff" | "JobSeeker" — API_DOCUMENTATION.md §3.1,
    /// what the frontend router uses to pick a landing page.</summary>
    public string Role { get; set; } = string.Empty;
    public Guid? OfficeId { get; set; }

    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

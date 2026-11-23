using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.Auth;

/// <summary>
/// Short-lived, single-purpose OTP code (DATABASE_DOCUMENTATION.md §3). Deliberately not an
/// <see cref="Tawzeef.Domain.Common.IEntity"/> — it is the one table explicitly exempt from the
/// platform's "retain everything" rule; purge consumed/expired rows on a schedule.
/// </summary>
public class OtpVerification
{
    public Guid Id { get; set; }

    /// <summary>Exactly one of PhoneNumber/Email is set per row — which one depends on the
    /// channel requested (AuthService.RequestOtpAsync for phone, RequestEmailOtpAsync for email,
    /// added 2026-11-23).</summary>
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string CodeHash { get; set; } = string.Empty;
    public OtpPurpose Purpose { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public byte AttemptCount { get; set; }

    /// <summary>
    /// Set once the code is verified — the opaque, single-use proof a register/reset call must
    /// present (API_DOCUMENTATION.md §3.1). Not the OTP code itself.
    /// </summary>
    public string? VerifiedToken { get; set; }
    public DateTime? VerifiedTokenExpiresAt { get; set; }
    public DateTime? VerifiedTokenUsedAt { get; set; }
}

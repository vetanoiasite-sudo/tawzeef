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

    public string PhoneNumber { get; set; } = string.Empty;
    public string CodeHash { get; set; } = string.Empty;
    public OtpPurpose Purpose { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public byte AttemptCount { get; set; }
}

using Microsoft.AspNetCore.Identity;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Identity;

/// <summary>
/// RBAC_PERMISSIONS.md §0 item 10: login is by <see cref="IdentityUser{TKey}.PhoneNumber"/>,
/// confirmed via OTP (see <see cref="Tawzeef.Domain.Entities.Auth.OtpVerification"/>) before
/// <see cref="IdentityUser{TKey}.PhoneNumberConfirmed"/> is set.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;

    /// <summary>Platform-level flag, orthogonal to office membership. RBAC_PERMISSIONS.md §1.</summary>
    public bool IsSuperAdmin { get; set; }

    public AccountStatus AccountStatus { get; set; } = AccountStatus.Active;
}

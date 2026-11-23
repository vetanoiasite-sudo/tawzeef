using System.ComponentModel.DataAnnotations;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.OfficeUsers;

/// <summary>API_DOCUMENTATION.md §3.3. OfficeId is only read for a SuperAdmin caller — an
/// OfficeAdmin always invites into their own office regardless of what's sent here.</summary>
public class InviteOfficeStaffDto
{
    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string MobileNumber { get; set; } = string.Empty;

    /// <summary>Set by the inviter, not the invitee — login is by email for every account now
    /// (office staff included), and there's no OTP step left to verify an invitee-entered one.</summary>
    [Required, MaxLength(200), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public OfficeRole Role { get; set; }

    /// <summary>Only meaningful when Role is OfficeStaff — an OfficeAdmin invite must leave this
    /// empty, since that role holds every permission automatically.</summary>
    public List<string> PermissionCodes { get; set; } = [];

    public Guid? OfficeId { get; set; }
}

public class UpdateOfficeUserPermissionsDto
{
    public List<string> PermissionCodes { get; set; } = [];
}

public class OfficeUserDto
{
    public Guid MembershipId { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string MembershipStatus { get; set; } = string.Empty;
    public List<string> PermissionCodes { get; set; } = [];
}

using Tawzeef.Domain.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.Offices;

/// <summary>BUSINESS_REQUIREMENTS.md §12.3: SuperAdmin invites the first OfficeAdmin; an
/// OfficeAdmin invites the rest. The invitee sets their own password on accept
/// (AuthService.AcceptStaffInviteAsync) and the real ApplicationUser/UserOfficeMembership rows are
/// created then, never here. Nobody but the invitee ever sets or sees their password.
/// <para>OTP phone verification was removed from this flow 2026-11-23 — a temporary product
/// decision, not a redesign (OtpPurpose.StaffInvite and the whole OTP request/verify endpoint
/// pair are untouched and still work, just unused here for now — see LoginDto's doc comment for
/// the matching login-side change). Email is required here (set by the inviter, not the invitee)
/// because login is by email for every account now, office staff included.</para></summary>
public class OfficeStaffInvite : IEntity
{
    public Guid Id { get; set; }

    public Guid OfficeId { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public OfficeRole Role { get; set; }

    /// <summary>Comma-separated PermissionCatalogue codes — only meaningful for OfficeStaff;
    /// empty for OfficeAdmin, who holds every permission automatically.</summary>
    public string PermissionCodesCsv { get; set; } = string.Empty;

    public Guid InvitedBy { get; set; }
    public DateTime? AcceptedAt { get; set; }

    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;
}

using Tawzeef.Domain.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.Offices;

/// <summary>BUSINESS_REQUIREMENTS.md §12.3: SuperAdmin invites the first OfficeAdmin; an
/// OfficeAdmin invites the rest. Mirrors the JobSeeker/Employer self-registration pattern — OTP
/// first (OtpPurpose.StaffInvite), then the invitee sets their own password on accept
/// (AuthService.AcceptStaffInviteAsync) and the real ApplicationUser/UserOfficeMembership rows are
/// created then, never here. Nobody but the invitee ever sets or sees their password.</summary>
public class OfficeStaffInvite : IEntity
{
    public Guid Id { get; set; }

    public Guid OfficeId { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
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

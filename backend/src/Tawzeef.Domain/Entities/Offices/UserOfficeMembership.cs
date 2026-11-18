using Tawzeef.Domain.Common;
using Tawzeef.Domain.Enums;
using Tawzeef.Domain.Identity;

namespace Tawzeef.Domain.Entities.Offices;

/// <summary>
/// Confirmed one office per account (RBAC_PERMISSIONS.md §0 item 4) — this table is still the
/// seam for a future multi-office account, so a second active row per user is a data change,
/// not a schema migration, if that rule is ever relaxed.
/// </summary>
public class UserOfficeMembership : IEntity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public Guid OfficeId { get; set; }
    public EmploymentOffice? Office { get; set; }

    public OfficeRole Role { get; set; }
    public MembershipStatus MembershipStatus { get; set; } = MembershipStatus.Invited;

    public ICollection<StaffPermissionGrant> PermissionGrants { get; set; } = new List<StaffPermissionGrant>();

    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;
}

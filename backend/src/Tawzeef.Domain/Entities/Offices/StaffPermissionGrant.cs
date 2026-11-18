using Tawzeef.Domain.Common;

namespace Tawzeef.Domain.Entities.Offices;

/// <summary>Data-driven OfficeStaff permission subset. RBAC_PERMISSIONS.md §1/§7.</summary>
public class StaffPermissionGrant : IEntity
{
    public Guid Id { get; set; }

    public Guid UserOfficeMembershipId { get; set; }
    public UserOfficeMembership? UserOfficeMembership { get; set; }

    public string PermissionCode { get; set; } = string.Empty;
    public PermissionDefinition? PermissionDefinition { get; set; }

    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;
}

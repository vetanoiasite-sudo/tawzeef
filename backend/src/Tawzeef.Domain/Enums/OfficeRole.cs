namespace Tawzeef.Domain.Enums;

/// <summary>
/// RBAC_PERMISSIONS.md §1: OfficeAdmin has the full permission set within its own office;
/// OfficeStaff gets a data-driven subset via <see cref="Tawzeef.Domain.Entities.Offices.StaffPermissionGrant"/>.
/// </summary>
public enum OfficeRole : byte
{
    OfficeAdmin = 0,
    OfficeStaff = 1,
}

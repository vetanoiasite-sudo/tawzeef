using Tawzeef.Application.Common;

namespace Tawzeef.Application.Features.OfficeUsers;

public interface IOfficeUserService
{
    Task<ServiceResult> InviteAsync(
        Guid inviterId, bool inviterIsSuperAdmin, Guid? inviterOfficeId, InviteOfficeStaffDto dto, CancellationToken ct = default);

    Task<ServiceResult<IReadOnlyList<OfficeUserDto>>> ListAsync(Guid officeId, CancellationToken ct = default);

    Task<ServiceResult<OfficeUserDto>> UpdatePermissionsAsync(
        Guid officeId, Guid membershipId, List<string> permissionCodes, CancellationToken ct = default);

    Task<ServiceResult> DeactivateAsync(Guid officeId, Guid actingUserId, Guid membershipId, CancellationToken ct = default);
}

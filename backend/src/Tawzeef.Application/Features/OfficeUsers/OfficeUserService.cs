using Microsoft.AspNetCore.Identity;
using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Offices;
using Tawzeef.Domain.Enums;
using Tawzeef.Domain.Identity;

namespace Tawzeef.Application.Features.OfficeUsers;

public class OfficeUserService(IUnitOfWork uow, UserManager<ApplicationUser> userManager) : IOfficeUserService
{
    public async Task<ServiceResult> InviteAsync(
        Guid inviterId, bool inviterIsSuperAdmin, Guid? inviterOfficeId, InviteOfficeStaffDto dto, CancellationToken ct = default)
    {
        Guid officeId;
        if (inviterIsSuperAdmin)
        {
            if (dto.OfficeId is null) return ServiceResult.Invalid("officeId is required when a SuperAdmin invites.");
            officeId = dto.OfficeId.Value;
        }
        else
        {
            // An OfficeAdmin can only ever invite into their own office — RBAC_PERMISSIONS.md
            // footnote 1 (never escalate by targeting another office).
            officeId = inviterOfficeId!.Value;
        }

        var office = await uow.Repository<EmploymentOffice>().FindAsync(o => o.Id == officeId && o.Status == OfficeStatus.Active, ct: ct);
        if (office is null) return ServiceResult.Invalid("Select a valid, active office.");

        var phoneNumber = dto.MobileNumber.Trim();
        if (await userManager.FindByNameAsync(phoneNumber) is not null)
            return ServiceResult.Conflict("This mobile number is already registered.");

        var hasPendingInvite = await uow.Repository<OfficeStaffInvite>().AnyAsync(
            i => i.MobileNumber == phoneNumber && i.AcceptedAt == null, ct);
        if (hasPendingInvite) return ServiceResult.Conflict("This mobile number already has a pending invite.");

        if (dto.Role == OfficeRole.OfficeAdmin && dto.PermissionCodes.Count > 0)
            return ServiceResult.Invalid("An OfficeAdmin holds every permission automatically — don't pass permissionCodes.");

        if (dto.Role == OfficeRole.OfficeStaff)
        {
            var invalid = ValidatePermissionCodes(dto.PermissionCodes);
            if (invalid is not null) return ServiceResult.Invalid(invalid);
        }

        var invite = new OfficeStaffInvite
        {
            Id = Guid.NewGuid(),
            OfficeId = officeId,
            MobileNumber = phoneNumber,
            FullName = dto.FullName.Trim(),
            Role = dto.Role,
            PermissionCodesCsv = string.Join(',', dto.PermissionCodes.Distinct()),
            InvitedBy = inviterId,
        };
        await uow.Repository<OfficeStaffInvite>().AddAsync(invite, ct);
        await uow.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<IReadOnlyList<OfficeUserDto>>> ListAsync(Guid officeId, CancellationToken ct = default)
    {
        var memberships = await uow.Repository<UserOfficeMembership>().ListAsync(m => m.OfficeId == officeId, ct: ct);
        var membershipIds = memberships.Select(m => m.Id).ToList();

        var grants = membershipIds.Count == 0
            ? []
            : await uow.Repository<StaffPermissionGrant>().ListAsync(g => membershipIds.Contains(g.UserOfficeMembershipId), ct: ct);
        var grantsByMembership = grants.GroupBy(g => g.UserOfficeMembershipId).ToDictionary(g => g.Key, g => g.Select(x => x.PermissionCode).ToList());

        var result = new List<OfficeUserDto>();
        foreach (var m in memberships)
        {
            var user = await userManager.FindByIdAsync(m.UserId.ToString());
            result.Add(new OfficeUserDto
            {
                MembershipId = m.Id,
                UserId = m.UserId,
                FullName = user?.FullName ?? string.Empty,
                MobileNumber = user?.PhoneNumber ?? string.Empty,
                Role = m.Role.ToString(),
                MembershipStatus = m.MembershipStatus.ToString(),
                PermissionCodes = grantsByMembership.GetValueOrDefault(m.Id, []),
            });
        }

        return ServiceResult<IReadOnlyList<OfficeUserDto>>.Success(result);
    }

    public async Task<ServiceResult<OfficeUserDto>> UpdatePermissionsAsync(
        Guid officeId, Guid membershipId, List<string> permissionCodes, CancellationToken ct = default)
    {
        var membership = await uow.Repository<UserOfficeMembership>().FindAsync(m => m.Id == membershipId && m.OfficeId == officeId, ct: ct);
        if (membership is null) return ServiceResult<OfficeUserDto>.NotFound("Office user not found.");
        if (membership.Role != OfficeRole.OfficeStaff)
            return ServiceResult<OfficeUserDto>.Invalid("Only an OfficeStaff member's permissions can be edited — OfficeAdmin already holds everything.");

        var invalid = ValidatePermissionCodes(permissionCodes);
        if (invalid is not null) return ServiceResult<OfficeUserDto>.Invalid(invalid);

        var existing = await uow.Repository<StaffPermissionGrant>().ListAsync(g => g.UserOfficeMembershipId == membershipId, ct: ct);
        foreach (var g in existing) uow.Repository<StaffPermissionGrant>().Delete(g);

        var distinctCodes = permissionCodes.Distinct().ToList();
        foreach (var code in distinctCodes)
        {
            await uow.Repository<StaffPermissionGrant>().AddAsync(new StaffPermissionGrant
            {
                Id = Guid.NewGuid(),
                UserOfficeMembershipId = membershipId,
                PermissionCode = code,
            }, ct);
        }

        await uow.SaveChangesAsync(ct);

        var user = await userManager.FindByIdAsync(membership.UserId.ToString());
        return ServiceResult<OfficeUserDto>.Success(new OfficeUserDto
        {
            MembershipId = membership.Id,
            UserId = membership.UserId,
            FullName = user?.FullName ?? string.Empty,
            MobileNumber = user?.PhoneNumber ?? string.Empty,
            Role = membership.Role.ToString(),
            MembershipStatus = membership.MembershipStatus.ToString(),
            PermissionCodes = distinctCodes,
        });
    }

    public async Task<ServiceResult> DeactivateAsync(Guid officeId, Guid actingUserId, Guid membershipId, CancellationToken ct = default)
    {
        var membership = await uow.Repository<UserOfficeMembership>().FindAsync(m => m.Id == membershipId && m.OfficeId == officeId, ct: ct);
        if (membership is null) return ServiceResult.NotFound("Office user not found.");

        if (membership.UserId == actingUserId)
            return ServiceResult.Invalid("You cannot revoke your own access.");

        if (membership.Role == OfficeRole.OfficeAdmin)
        {
            var otherActiveAdmins = await uow.Repository<UserOfficeMembership>().CountAsync(
                m => m.OfficeId == officeId && m.Role == OfficeRole.OfficeAdmin
                    && m.MembershipStatus == MembershipStatus.Active && m.Id != membershipId,
                ct);
            if (otherActiveAdmins == 0)
                return ServiceResult.Invalid("This is the office's only administrator — invite another OfficeAdmin before revoking this one.");
        }

        membership.MembershipStatus = MembershipStatus.Revoked;
        uow.Repository<UserOfficeMembership>().Update(membership);

        var user = await userManager.FindByIdAsync(membership.UserId.ToString());
        if (user is not null)
        {
            user.AccountStatus = AccountStatus.Deactivated;
            await userManager.UpdateAsync(user);
        }

        await uow.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    private static string? ValidatePermissionCodes(List<string> permissionCodes)
    {
        var validCodes = PermissionCatalogue.All.Select(p => p.Code).ToHashSet();
        var unknown = permissionCodes.Where(c => !validCodes.Contains(c)).ToList();
        if (unknown.Count > 0) return $"Unknown permission code(s): {string.Join(", ", unknown)}.";

        var adminOnly = permissionCodes.Where(c => PermissionCatalogue.OfficeAdminOnly.Contains(c)).ToList();
        if (adminOnly.Count > 0) return $"These permissions are OfficeAdmin-only and can't be granted: {string.Join(", ", adminOnly)}.";

        return null;
    }
}

using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Auth;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Entities.JobSeekers;
using Tawzeef.Domain.Entities.Offices;
using Tawzeef.Domain.Enums;
using Tawzeef.Domain.Identity;

namespace Tawzeef.Application.Features.Auth;

public class AuthService(
    IUnitOfWork uow,
    UserManager<ApplicationUser> userManager,
    IOtpSender otpSender,
    IJwtTokenGenerator tokenGenerator) : IAuthService
{
    private const int OtpLifetimeMinutes = 5;
    private const int OtpTokenLifetimeMinutes = 15;
    private const byte MaxOtpAttempts = 5;

    public async Task<ServiceResult> RequestOtpAsync(RequestOtpDto dto, CancellationToken ct = default)
    {
        var phoneNumber = dto.PhoneNumber.Trim();
        var code = GenerateNumericCode(6);

        await uow.Repository<OtpVerification>().AddAsync(new OtpVerification
        {
            Id = Guid.NewGuid(),
            PhoneNumber = phoneNumber,
            CodeHash = Hash(code),
            Purpose = dto.Purpose,
            ExpiresAt = DateTime.UtcNow.AddMinutes(OtpLifetimeMinutes),
        }, ct);
        await uow.SaveChangesAsync(ct);

        await otpSender.SendAsync(phoneNumber, code, ct);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult<OtpVerifiedDto>> VerifyOtpAsync(VerifyOtpDto dto, CancellationToken ct = default)
    {
        var phoneNumber = dto.PhoneNumber.Trim();
        var candidates = await uow.Repository<OtpVerification>().ListAsync(
            o => o.PhoneNumber == phoneNumber && o.Purpose == dto.Purpose && o.ConsumedAt == null && o.ExpiresAt > DateTime.UtcNow,
            ct: ct);
        var otp = candidates.OrderByDescending(o => o.ExpiresAt).FirstOrDefault();

        if (otp is null) return ServiceResult<OtpVerifiedDto>.Invalid("No active code for this number — request a new one.");
        if (otp.AttemptCount >= MaxOtpAttempts) return ServiceResult<OtpVerifiedDto>.Invalid("Too many attempts — request a new code.");

        if (otp.CodeHash != Hash(dto.Code.Trim()))
        {
            otp.AttemptCount++;
            uow.Repository<OtpVerification>().Update(otp);
            await uow.SaveChangesAsync(ct);
            return ServiceResult<OtpVerifiedDto>.Invalid("Incorrect code.");
        }

        otp.ConsumedAt = DateTime.UtcNow;
        otp.VerifiedToken = GenerateOpaqueToken();
        otp.VerifiedTokenExpiresAt = DateTime.UtcNow.AddMinutes(OtpTokenLifetimeMinutes);
        uow.Repository<OtpVerification>().Update(otp);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<OtpVerifiedDto>.Success(new OtpVerifiedDto { OtpToken = otp.VerifiedToken });
    }

    public async Task<ServiceResult<AuthResultDto>> RegisterJobSeekerAsync(RegisterJobSeekerDto dto, CancellationToken ct = default)
    {
        var phoneNumber = dto.MobileNumber.Trim();

        var otpMatches = await uow.Repository<OtpVerification>().ListAsync(
            o => o.PhoneNumber == phoneNumber && o.Purpose == OtpPurpose.Registration
                 && o.VerifiedToken == dto.OtpToken && o.VerifiedTokenUsedAt == null
                 && o.VerifiedTokenExpiresAt > DateTime.UtcNow,
            ct: ct);
        var verifiedOtp = otpMatches.FirstOrDefault();
        if (verifiedOtp is null)
            return ServiceResult<AuthResultDto>.Invalid("Phone verification has expired — verify the number again.");

        var office = await uow.Repository<EmploymentOffice>().FindAsync(o => o.Id == dto.OfficeId && o.Status == OfficeStatus.Active, ct: ct);
        if (office is null) return ServiceResult<AuthResultDto>.Invalid("Select a valid office.");

        if (await userManager.FindByNameAsync(phoneNumber) is not null)
            return ServiceResult<AuthResultDto>.Conflict("This mobile number is already registered.");

        var email = dto.Email.Trim();
        if (await userManager.FindByEmailAsync(email) is not null)
            return ServiceResult<AuthResultDto>.Conflict("This email is already registered.");

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = phoneNumber,
            PhoneNumber = phoneNumber,
            PhoneNumberConfirmed = true, // just proved via OTP
            Email = email,
            FullName = dto.FullName.Trim(),
        };

        var createResult = await userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
            return ServiceResult<AuthResultDto>.Invalid(createResult.Errors.Select(e => e.Description));

        var profile = new JobSeekerProfile
        {
            Id = Guid.NewGuid(),
            OfficeId = office.Id,
            UserId = user.Id,
            FullName = user.FullName,
            MobileNumber = phoneNumber,
            Email = dto.Email,
            ReviewStatus = JobSeekerReviewStatus.Draft,
        };
        await uow.Repository<JobSeekerProfile>().AddAsync(profile, ct);

        verifiedOtp.VerifiedTokenUsedAt = DateTime.UtcNow;
        uow.Repository<OtpVerification>().Update(verifiedOtp);

        await uow.SaveChangesAsync(ct);

        var (accessToken, expiresAt) = tokenGenerator.GenerateToken(user.Id, user.FullName, isSuperAdmin: false, officeId: null);
        return ServiceResult<AuthResultDto>.Success(new AuthResultDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            JobSeekerProfileId = profile.Id,
            Role = "JobSeeker",
            AccessToken = accessToken,
            ExpiresAt = expiresAt,
        });
    }

    public async Task<ServiceResult<AuthResultDto>> RegisterEmployerAsync(RegisterEmployerDto dto, CancellationToken ct = default)
    {
        var phoneNumber = dto.MobileNumber.Trim();

        var otpMatches = await uow.Repository<OtpVerification>().ListAsync(
            o => o.PhoneNumber == phoneNumber && o.Purpose == OtpPurpose.Registration
                 && o.VerifiedToken == dto.OtpToken && o.VerifiedTokenUsedAt == null
                 && o.VerifiedTokenExpiresAt > DateTime.UtcNow,
            ct: ct);
        var verifiedOtp = otpMatches.FirstOrDefault();
        if (verifiedOtp is null)
            return ServiceResult<AuthResultDto>.Invalid("Phone verification has expired — verify the number again.");

        var office = await uow.Repository<EmploymentOffice>().FindAsync(o => o.Id == dto.OfficeId && o.Status == OfficeStatus.Active, ct: ct);
        if (office is null) return ServiceResult<AuthResultDto>.Invalid("Select a valid office.");

        if (await userManager.FindByNameAsync(phoneNumber) is not null)
            return ServiceResult<AuthResultDto>.Conflict("This mobile number is already registered.");

        var email = dto.Email.Trim();
        if (await userManager.FindByEmailAsync(email) is not null)
            return ServiceResult<AuthResultDto>.Conflict("This email is already registered.");

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = phoneNumber,
            PhoneNumber = phoneNumber,
            PhoneNumberConfirmed = true, // just proved via OTP
            Email = email,
            FullName = dto.RepresentativeName.Trim(),
        };

        var createResult = await userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
            return ServiceResult<AuthResultDto>.Invalid(createResult.Errors.Select(e => e.Description));

        var employer = new Employer
        {
            Id = Guid.NewGuid(),
            OfficeId = office.Id,
            UserId = user.Id,
            LegalName = dto.LegalName.Trim(),
            DisplayName = dto.DisplayName.Trim(),
            RepresentativeName = user.FullName,
            CompanyEmail = email,
            VerificationStatus = EmployerVerificationStatus.Draft,
            CooperationStatus = EmployerCooperationStatus.Active,
        };
        await uow.Repository<Employer>().AddAsync(employer, ct);

        verifiedOtp.VerifiedTokenUsedAt = DateTime.UtcNow;
        uow.Repository<OtpVerification>().Update(verifiedOtp);

        await uow.SaveChangesAsync(ct);

        var (accessToken, expiresAt) = tokenGenerator.GenerateToken(user.Id, user.FullName, isSuperAdmin: false, officeId: null);
        return ServiceResult<AuthResultDto>.Success(new AuthResultDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            EmployerId = employer.Id,
            Role = "Employer",
            AccessToken = accessToken,
            ExpiresAt = expiresAt,
        });
    }

    public async Task<ServiceResult<AuthResultDto>> AcceptStaffInviteAsync(AcceptOfficeInviteDto dto, CancellationToken ct = default)
    {
        var phoneNumber = dto.MobileNumber.Trim();

        var otpMatches = await uow.Repository<OtpVerification>().ListAsync(
            o => o.PhoneNumber == phoneNumber && o.Purpose == OtpPurpose.StaffInvite
                 && o.VerifiedToken == dto.OtpToken && o.VerifiedTokenUsedAt == null
                 && o.VerifiedTokenExpiresAt > DateTime.UtcNow,
            ct: ct);
        var verifiedOtp = otpMatches.FirstOrDefault();
        if (verifiedOtp is null)
            return ServiceResult<AuthResultDto>.Invalid("Phone verification has expired — verify the number again.");

        var pendingInvites = await uow.Repository<OfficeStaffInvite>().ListAsync(
            i => i.MobileNumber == phoneNumber && i.AcceptedAt == null, ct: ct);
        var invite = pendingInvites.OrderByDescending(i => i.CreationDate).FirstOrDefault();
        if (invite is null) return ServiceResult<AuthResultDto>.NotFound("No pending invite for this number.");

        if (await userManager.FindByNameAsync(phoneNumber) is not null)
            return ServiceResult<AuthResultDto>.Conflict("This mobile number is already registered.");

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = phoneNumber,
            PhoneNumber = phoneNumber,
            PhoneNumberConfirmed = true, // just proved via OTP
            FullName = invite.FullName,
        };

        var createResult = await userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
            return ServiceResult<AuthResultDto>.Invalid(createResult.Errors.Select(e => e.Description));

        var membership = new UserOfficeMembership
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            OfficeId = invite.OfficeId,
            Role = invite.Role,
            MembershipStatus = MembershipStatus.Active,
        };
        await uow.Repository<UserOfficeMembership>().AddAsync(membership, ct);

        var permissionCodes = invite.Role == OfficeRole.OfficeAdmin
            ? PermissionCatalogue.All.Select(p => p.Code).ToList()
            : invite.PermissionCodesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

        if (invite.Role == OfficeRole.OfficeStaff)
        {
            foreach (var code in permissionCodes)
            {
                await uow.Repository<StaffPermissionGrant>().AddAsync(new StaffPermissionGrant
                {
                    Id = Guid.NewGuid(),
                    UserOfficeMembershipId = membership.Id,
                    PermissionCode = code,
                }, ct);
            }
        }

        invite.AcceptedAt = DateTime.UtcNow;
        uow.Repository<OfficeStaffInvite>().Update(invite);

        verifiedOtp.VerifiedTokenUsedAt = DateTime.UtcNow;
        uow.Repository<OtpVerification>().Update(verifiedOtp);

        await uow.SaveChangesAsync(ct);

        var (accessToken, expiresAt) = tokenGenerator.GenerateToken(user.Id, user.FullName, isSuperAdmin: false, officeId: membership.OfficeId);
        return ServiceResult<AuthResultDto>.Success(new AuthResultDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Role = invite.Role.ToString(),
            OfficeId = membership.OfficeId,
            PermissionCodes = permissionCodes,
            AccessToken = accessToken,
            ExpiresAt = expiresAt,
        });
    }

    public async Task<ServiceResult<AuthResultDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var identifier = dto.Identifier.Trim();
        // Email first (JobSeeker/Employer accounts), then phone/username (OfficeAdmin/OfficeStaff/
        // SuperAdmin, which never collect an email) — see LoginDto's doc comment.
        var user = await userManager.FindByEmailAsync(identifier) ?? await userManager.FindByNameAsync(identifier);

        if (user is null)
        {
            var emp = await uow.Repository<Employer>().FindAsync(e => e.CompanyEmail == identifier, ct: ct);
            if (emp is not null)
            {
                user = await userManager.FindByIdAsync(emp.UserId.ToString());
            }
        }
        if (user is null)
        {
            var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.Email == identifier, ct: ct);
            if (profile is not null)
            {
                user = await userManager.FindByIdAsync(profile.UserId.ToString());
            }
        }
        if (user is null || !await userManager.CheckPasswordAsync(user, dto.Password))
            return ServiceResult<AuthResultDto>.Invalid("Incorrect email/phone number or password.");

        if (user.AccountStatus != AccountStatus.Active)
            return ServiceResult<AuthResultDto>.Forbidden("This account is not active.");

        var membership = await uow.Repository<Tawzeef.Domain.Entities.Offices.UserOfficeMembership>().FindAsync(
            m => m.UserId == user.Id && m.MembershipStatus == MembershipStatus.Active, ct: ct);

        string role;
        Guid? jobSeekerProfileId = null;
        Guid? employerId = null;
        if (user.IsSuperAdmin)
        {
            role = "SuperAdmin";
        }
        else if (membership is not null)
        {
            role = membership.Role.ToString();
        }
        else
        {
            var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.UserId == user.Id, ct: ct);
            if (profile is not null)
            {
                role = "JobSeeker";
                jobSeekerProfileId = profile.Id;
            }
            else
            {
                var employer = await uow.Repository<Employer>().FindAsync(e => e.UserId == user.Id, ct: ct);
                role = "Employer";
                employerId = employer?.Id;
            }
        }

        var permissionCodes = await ResolvePermissionCodesAsync(membership, ct);

        var (accessToken, expiresAt) = tokenGenerator.GenerateToken(user.Id, user.FullName, user.IsSuperAdmin, membership?.OfficeId);
        return ServiceResult<AuthResultDto>.Success(new AuthResultDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            JobSeekerProfileId = jobSeekerProfileId,
            EmployerId = employerId,
            Role = role,
            OfficeId = membership?.OfficeId,
            PermissionCodes = permissionCodes,
            AccessToken = accessToken,
            ExpiresAt = expiresAt,
        });
    }

    /// <summary>Mirrors Tawzeef.Infrastructure.Authorization.PermissionChecker's own resolution —
    /// OfficeAdmin holds every catalogued code within their office, OfficeStaff only the
    /// data-driven subset a StaffPermissionGrant row lists (RBAC_PERMISSIONS.md §1/§7).</summary>
    private async Task<List<string>> ResolvePermissionCodesAsync(Tawzeef.Domain.Entities.Offices.UserOfficeMembership? membership, CancellationToken ct)
    {
        if (membership is null) return [];
        if (membership.Role == OfficeRole.OfficeAdmin)
            return Tawzeef.Domain.Common.PermissionCatalogue.All.Select(p => p.Code).ToList();

        var grants = await uow.Repository<Tawzeef.Domain.Entities.Offices.StaffPermissionGrant>().ListAsync(
            g => g.UserOfficeMembershipId == membership.Id, ct: ct);

        // Mirrors PermissionChecker.UniversalOfficeMemberPermissions — codes OfficeStaff holds
        // from active membership alone, with no StaffPermissionGrant row. Without this, the login
        // response's permissionCodes (what the frontend's useAccess reads) would omit them for a
        // zero-grant staff member even though the backend's own filter already allows the call.
        return grants.Select(g => g.PermissionCode)
            .Append("notifications.viewown")
            .Append("dashboards.view")
            .Distinct()
            .ToList();
    }

    private static string GenerateNumericCode(int length)
    {
        Span<byte> bytes = stackalloc byte[length];
        RandomNumberGenerator.Fill(bytes);
        var chars = new char[length];
        for (var i = 0; i < length; i++) chars[i] = (char)('0' + bytes[i] % 10);
        return new string(chars);
    }

    private static string GenerateOpaqueToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).Replace('+', '-').Replace('/', '_').TrimEnd('=');

    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}

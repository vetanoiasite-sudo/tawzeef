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
    IEmailSender emailSender,
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

        if (otp is null) return ServiceResult<OtpVerifiedDto>.Invalid("لا يوجد رمز نشط لهذا الرقم — اطلب رمزًا جديدًا.");
        if (otp.AttemptCount >= MaxOtpAttempts) return ServiceResult<OtpVerifiedDto>.Invalid("عدد المحاولات كبير جدًا — اطلب رمزًا جديدًا.");

        if (otp.CodeHash != Hash(dto.Code.Trim()))
        {
            otp.AttemptCount++;
            uow.Repository<OtpVerification>().Update(otp);
            await uow.SaveChangesAsync(ct);
            return ServiceResult<OtpVerifiedDto>.Invalid("الرمز غير صحيح.");
        }

        otp.ConsumedAt = DateTime.UtcNow;
        otp.VerifiedToken = GenerateOpaqueToken();
        otp.VerifiedTokenExpiresAt = DateTime.UtcNow.AddMinutes(OtpTokenLifetimeMinutes);
        uow.Repository<OtpVerification>().Update(otp);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<OtpVerifiedDto>.Success(new OtpVerifiedDto { OtpToken = otp.VerifiedToken });
    }

    /// <summary>Same flow as RequestOtpAsync, over email instead of SMS — added 2026-11-23 so
    /// registration can require a verified email (RegisterJobSeekerDto.OtpToken's doc comment).</summary>
    public async Task<ServiceResult> RequestEmailOtpAsync(RequestEmailOtpDto dto, CancellationToken ct = default)
    {
        var email = dto.Email.Trim();
        var code = GenerateNumericCode(6);

        await uow.Repository<OtpVerification>().AddAsync(new OtpVerification
        {
            Id = Guid.NewGuid(),
            Email = email,
            CodeHash = Hash(code),
            Purpose = dto.Purpose,
            ExpiresAt = DateTime.UtcNow.AddMinutes(OtpLifetimeMinutes),
        }, ct);
        await uow.SaveChangesAsync(ct);

        await emailSender.SendAsync(email, "رمز تأكيد البريد الإلكتروني", $"رمز التأكيد الخاص بك هو: {code}");
        return ServiceResult.Success();
    }

    /// <summary>Same flow as VerifyOtpAsync, over email — see RequestEmailOtpAsync's doc comment.</summary>
    public async Task<ServiceResult<OtpVerifiedDto>> VerifyEmailOtpAsync(VerifyEmailOtpDto dto, CancellationToken ct = default)
    {
        var email = dto.Email.Trim();
        var candidates = await uow.Repository<OtpVerification>().ListAsync(
            o => o.Email == email && o.Purpose == dto.Purpose && o.ConsumedAt == null && o.ExpiresAt > DateTime.UtcNow,
            ct: ct);
        var otp = candidates.OrderByDescending(o => o.ExpiresAt).FirstOrDefault();

        if (otp is null) return ServiceResult<OtpVerifiedDto>.Invalid("لا يوجد رمز نشط لهذا البريد الإلكتروني — اطلب رمزًا جديدًا.");
        if (otp.AttemptCount >= MaxOtpAttempts) return ServiceResult<OtpVerifiedDto>.Invalid("عدد المحاولات كبير جدًا — اطلب رمزًا جديدًا.");

        if (otp.CodeHash != Hash(dto.Code.Trim()))
        {
            otp.AttemptCount++;
            uow.Repository<OtpVerification>().Update(otp);
            await uow.SaveChangesAsync(ct);
            return ServiceResult<OtpVerifiedDto>.Invalid("الرمز غير صحيح.");
        }

        otp.ConsumedAt = DateTime.UtcNow;
        otp.VerifiedToken = GenerateOpaqueToken();
        otp.VerifiedTokenExpiresAt = DateTime.UtcNow.AddMinutes(OtpTokenLifetimeMinutes);
        uow.Repository<OtpVerification>().Update(otp);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<OtpVerifiedDto>.Success(new OtpVerifiedDto { OtpToken = otp.VerifiedToken });
    }

    /// <summary>The forgot-password flow's last step — ResetPasswordDto's doc comment explains why
    /// request/verify reuse the existing email-OTP endpoints and only this step is new.</summary>
    public async Task<ServiceResult> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken ct = default)
    {
        var email = dto.Email.Trim();

        var otp = await uow.Repository<OtpVerification>().FindAsync(
            o => o.Email == email
                && o.Purpose == OtpPurpose.PasswordReset
                && o.VerifiedToken == dto.OtpToken.Trim()
                && o.VerifiedTokenUsedAt == null
                && o.VerifiedTokenExpiresAt > DateTime.UtcNow,
            ct: ct);
        if (otp is null) return ServiceResult.Invalid("يجب تأكيد البريد الإلكتروني أولًا.");

        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return ServiceResult.NotFound("لا يوجد حساب بهذا البريد الإلكتروني.");

        // Not GeneratePasswordResetTokenAsync/ResetPasswordAsync — see ResetPasswordDto's doc
        // comment. RemovePasswordAsync + AddPasswordAsync needs no token provider.
        var removeResult = await userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded)
            return ServiceResult.Invalid(removeResult.Errors.Select(e => e.Description));

        var addResult = await userManager.AddPasswordAsync(user, dto.NewPassword);
        if (!addResult.Succeeded)
            return ServiceResult.Invalid(addResult.Errors.Select(e => e.Description));

        otp.VerifiedTokenUsedAt = DateTime.UtcNow;
        uow.Repository<OtpVerification>().Update(otp);
        await uow.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }

    /// <summary>Consumes the opaque token from VerifyEmailOtpAsync — RegisterJobSeekerAsync/
    /// RegisterEmployerAsync both call this before creating the account, so an account can't be
    /// created without first proving the email was actually verified for this same email and
    /// Purpose.Registration.</summary>
    private async Task<string?> ConsumeEmailOtpTokenAsync(string email, string otpToken, CancellationToken ct)
    {
        var otp = await uow.Repository<OtpVerification>().FindAsync(
            o => o.Email == email
                && o.Purpose == OtpPurpose.Registration
                && o.VerifiedToken == otpToken
                && o.VerifiedTokenUsedAt == null
                && o.VerifiedTokenExpiresAt > DateTime.UtcNow,
            ct: ct);
        if (otp is null) return "يجب تأكيد البريد الإلكتروني أولًا.";

        otp.VerifiedTokenUsedAt = DateTime.UtcNow;
        uow.Repository<OtpVerification>().Update(otp);
        return null;
    }

    public async Task<ServiceResult<AuthResultDto>> RegisterJobSeekerAsync(RegisterJobSeekerDto dto, CancellationToken ct = default)
    {
        var phoneNumber = dto.MobileNumber.Trim();

        // No phone OTP step — removed 2026-11-23 (LoginDto's doc comment). Email OTP is required
        // instead, added the same day — ConsumeEmailOtpTokenAsync's doc comment. Unlike an
        // Employer, the job seeker still picks office(s) right here at registration (confirmed
        // 2026-11-24 — JobSeekerOfficeApplication's doc comment): it's the office(s) that will
        // review their documents, so there's no "apply later" step to defer it to — though more can
        // still be added afterward via JobSeekersController.SubmitToOffices.
        var offices = dto.AllOffices
            ? await uow.Repository<EmploymentOffice>().ListAsync(o => o.Status == OfficeStatus.Active, ct: ct)
            : await uow.Repository<EmploymentOffice>().ListAsync(o => dto.OfficeIds.Contains(o.Id) && o.Status == OfficeStatus.Active, ct: ct);
        if (offices.Count == 0) return ServiceResult<AuthResultDto>.Invalid("اختر مكتب توظيف صالح واحدًا على الأقل.");

        if (await userManager.FindByNameAsync(phoneNumber) is not null)
            return ServiceResult<AuthResultDto>.Conflict("رقم الهاتف هذا مسجل بالفعل.");

        var email = dto.Email.Trim();
        if (await userManager.FindByEmailAsync(email) is not null)
            return ServiceResult<AuthResultDto>.Conflict("هذا البريد الإلكتروني مسجل بالفعل.");

        var otpError = await ConsumeEmailOtpTokenAsync(email, dto.OtpToken.Trim(), ct);
        if (otpError is not null) return ServiceResult<AuthResultDto>.Invalid(otpError);

        var nationalId = dto.NationalId.Trim();
        if (await uow.Repository<JobSeekerProfile>().FindAsync(p => p.NationalId == nationalId, ct: ct) is not null)
            return ServiceResult<AuthResultDto>.Conflict("هذا الرقم القومي مسجل بالفعل.");

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = phoneNumber,
            PhoneNumber = phoneNumber,
            PhoneNumberConfirmed = false, // no OTP step anymore — unverified until that comes back
            Email = email,
            FullName = dto.FullName.Trim(),
        };

        var createResult = await userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
            return ServiceResult<AuthResultDto>.Invalid(createResult.Errors.Select(e => e.Description));

        var profile = new JobSeekerProfile
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            FullName = user.FullName,
            NationalId = nationalId,
            MobileNumber = phoneNumber,
            Email = dto.Email,
        };
        await uow.Repository<JobSeekerProfile>().AddAsync(profile, ct);

        foreach (var office in offices)
        {
            await uow.Repository<JobSeekerOfficeApplication>().AddAsync(new JobSeekerOfficeApplication
            {
                Id = Guid.NewGuid(),
                JobSeekerId = profile.Id,
                OfficeId = office.Id,
                ReviewStatus = JobSeekerReviewStatus.Draft,
            }, ct);
        }

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

        // No OTP verification step here — removed 2026-11-23, temporary (LoginDto's doc comment).
        // Office choice happens right here at registration, same as RegisterJobSeekerAsync
        // (confirmed 2026-11-24 — RegisterEmployerDto's doc comment). More offices can still be
        // added later via EmployersController.SubmitToOffices.
        var offices = dto.AllOffices
            ? await uow.Repository<EmploymentOffice>().ListAsync(o => o.Status == OfficeStatus.Active, ct: ct)
            : await uow.Repository<EmploymentOffice>().ListAsync(o => dto.OfficeIds.Contains(o.Id) && o.Status == OfficeStatus.Active, ct: ct);
        if (offices.Count == 0) return ServiceResult<AuthResultDto>.Invalid("اختر مكتب توظيف صالح واحدًا على الأقل.");

        if (await userManager.FindByNameAsync(phoneNumber) is not null)
            return ServiceResult<AuthResultDto>.Conflict("رقم الهاتف هذا مسجل بالفعل.");

        var email = dto.Email.Trim();
        if (await userManager.FindByEmailAsync(email) is not null)
            return ServiceResult<AuthResultDto>.Conflict("هذا البريد الإلكتروني مسجل بالفعل.");

        var otpError = await ConsumeEmailOtpTokenAsync(email, dto.OtpToken.Trim(), ct);
        if (otpError is not null) return ServiceResult<AuthResultDto>.Invalid(otpError);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = phoneNumber,
            PhoneNumber = phoneNumber,
            PhoneNumberConfirmed = false, // no OTP step anymore — unverified until that comes back
            Email = email,
            FullName = dto.RepresentativeName.Trim(),
        };

        var createResult = await userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
            return ServiceResult<AuthResultDto>.Invalid(createResult.Errors.Select(e => e.Description));

        var employer = new Employer
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            LegalName = dto.LegalName.Trim(),
            DisplayName = dto.DisplayName.Trim(),
            RepresentativeName = user.FullName,
            CompanyEmail = email,
        };
        await uow.Repository<Employer>().AddAsync(employer, ct);

        foreach (var office in offices)
        {
            await uow.Repository<EmployerOfficeApplication>().AddAsync(new EmployerOfficeApplication
            {
                Id = Guid.NewGuid(),
                EmployerId = employer.Id,
                OfficeId = office.Id,
                VerificationStatus = EmployerVerificationStatus.Draft,
            }, ct);
        }

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

        // No OTP verification step here — removed 2026-11-23, temporary (LoginDto's doc comment).
        // MobileNumber alone identifies which pending invite this is (OfficeStaffInvite doc comment).
        var pendingInvites = await uow.Repository<OfficeStaffInvite>().ListAsync(
            i => i.MobileNumber == phoneNumber && i.AcceptedAt == null, ct: ct);
        var invite = pendingInvites.OrderByDescending(i => i.CreationDate).FirstOrDefault();
        if (invite is null) return ServiceResult<AuthResultDto>.NotFound("لا توجد دعوة معلّقة لهذا الرقم.");

        if (await userManager.FindByNameAsync(phoneNumber) is not null)
            return ServiceResult<AuthResultDto>.Conflict("رقم الهاتف هذا مسجل بالفعل.");

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = phoneNumber,
            PhoneNumber = phoneNumber,
            PhoneNumberConfirmed = false, // no OTP step anymore — unverified until that comes back
            Email = invite.Email, // set by the inviter — see InviteOfficeStaffDto's doc comment
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
        // Email only — no phone/username fallback. Login is by email for every account type now
        // (office staff included, since every invite carries one), per explicit product decision;
        // phone-based login is deliberately out until OTP comes back (LoginDto's doc comment).
        var user = await userManager.FindByEmailAsync(identifier);

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
            return ServiceResult<AuthResultDto>.Invalid("البريد الإلكتروني/رقم الهاتف أو كلمة المرور غير صحيحة.");

        if (user.AccountStatus != AccountStatus.Active)
            return ServiceResult<AuthResultDto>.Forbidden("هذا الحساب غير مفعّل.");

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

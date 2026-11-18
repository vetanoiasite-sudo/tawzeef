using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Tawzeef.Application.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Auth;
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

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = phoneNumber,
            PhoneNumber = phoneNumber,
            PhoneNumberConfirmed = true, // just proved via OTP
            Email = dto.Email,
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

    public async Task<ServiceResult<AuthResultDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var user = await userManager.FindByNameAsync(dto.Identifier.Trim());
        if (user is null || !await userManager.CheckPasswordAsync(user, dto.Password))
            return ServiceResult<AuthResultDto>.Invalid("Incorrect phone number or password.");

        if (user.AccountStatus != AccountStatus.Active)
            return ServiceResult<AuthResultDto>.Forbidden("This account is not active.");

        var membership = await uow.Repository<Tawzeef.Domain.Entities.Offices.UserOfficeMembership>().FindAsync(
            m => m.UserId == user.Id && m.MembershipStatus == MembershipStatus.Active, ct: ct);

        string role;
        Guid? jobSeekerProfileId = null;
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
            role = "JobSeeker";
            jobSeekerProfileId = profile?.Id;
        }

        var (accessToken, expiresAt) = tokenGenerator.GenerateToken(user.Id, user.FullName, user.IsSuperAdmin, membership?.OfficeId);
        return ServiceResult<AuthResultDto>.Success(new AuthResultDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            JobSeekerProfileId = jobSeekerProfileId,
            Role = role,
            OfficeId = membership?.OfficeId,
            AccessToken = accessToken,
            ExpiresAt = expiresAt,
        });
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

using Tawzeef.Application.Common;

namespace Tawzeef.Application.Features.Auth;

public interface IAuthService
{
    Task<ServiceResult> RequestOtpAsync(RequestOtpDto dto, CancellationToken ct = default);
    Task<ServiceResult<OtpVerifiedDto>> VerifyOtpAsync(VerifyOtpDto dto, CancellationToken ct = default);
    Task<ServiceResult> RequestEmailOtpAsync(RequestEmailOtpDto dto, CancellationToken ct = default);
    Task<ServiceResult<OtpVerifiedDto>> VerifyEmailOtpAsync(VerifyEmailOtpDto dto, CancellationToken ct = default);
    Task<ServiceResult> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken ct = default);
    Task<ServiceResult<AuthResultDto>> RegisterJobSeekerAsync(RegisterJobSeekerDto dto, CancellationToken ct = default);
    Task<ServiceResult<AuthResultDto>> RegisterEmployerAsync(RegisterEmployerDto dto, CancellationToken ct = default);
    Task<ServiceResult<AuthResultDto>> AcceptStaffInviteAsync(AcceptOfficeInviteDto dto, CancellationToken ct = default);
    Task<ServiceResult<AuthResultDto>> LoginAsync(LoginDto dto, CancellationToken ct = default);
}

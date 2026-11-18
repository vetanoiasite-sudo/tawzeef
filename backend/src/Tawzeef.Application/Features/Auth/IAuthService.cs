using Tawzeef.Application.Common;

namespace Tawzeef.Application.Features.Auth;

public interface IAuthService
{
    Task<ServiceResult> RequestOtpAsync(RequestOtpDto dto, CancellationToken ct = default);
    Task<ServiceResult<OtpVerifiedDto>> VerifyOtpAsync(VerifyOtpDto dto, CancellationToken ct = default);
    Task<ServiceResult<AuthResultDto>> RegisterJobSeekerAsync(RegisterJobSeekerDto dto, CancellationToken ct = default);
    Task<ServiceResult<AuthResultDto>> LoginAsync(LoginDto dto, CancellationToken ct = default);
}

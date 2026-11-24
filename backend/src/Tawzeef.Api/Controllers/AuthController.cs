using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.Auth;

namespace Tawzeef.Api.Controllers;

[Route("api/auth")]
[AllowAnonymous]
public class AuthController(IAuthService service) : ApiControllerBase
{
    [HttpPost("otp/request")]
    public async Task<IActionResult> RequestOtp([FromBody] RequestOtpDto dto, CancellationToken ct)
        => Respond(await service.RequestOtpAsync(dto, ct));

    [HttpPost("otp/verify")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto, CancellationToken ct)
        => Respond(await service.VerifyOtpAsync(dto, ct));

    [HttpPost("otp/email/request")]
    public async Task<IActionResult> RequestEmailOtp([FromBody] RequestEmailOtpDto dto, CancellationToken ct)
        => Respond(await service.RequestEmailOtpAsync(dto, ct));

    [HttpPost("otp/email/verify")]
    public async Task<IActionResult> VerifyEmailOtp([FromBody] VerifyEmailOtpDto dto, CancellationToken ct)
        => Respond(await service.VerifyEmailOtpAsync(dto, ct));

    /// <summary>Forgot-password: the frontend calls otp/email/request and otp/email/verify above
    /// (with Purpose: "PasswordReset") to get a VerifiedToken, then this to actually change the
    /// password — any account with an email on file, any role (ResetPasswordDto's doc comment).</summary>
    [HttpPost("password/reset")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken ct)
        => Respond(await service.ResetPasswordAsync(dto, ct));

    [HttpPost("register/job-seeker")]
    public async Task<IActionResult> RegisterJobSeeker([FromBody] RegisterJobSeekerDto dto, CancellationToken ct)
        => Respond(await service.RegisterJobSeekerAsync(dto, ct));

    [HttpPost("register/employer")]
    public async Task<IActionResult> RegisterEmployer([FromBody] RegisterEmployerDto dto, CancellationToken ct)
        => Respond(await service.RegisterEmployerAsync(dto, ct));

    /// <summary>Only reachable after an OfficeAdmin/SuperAdmin invite — see
    /// OfficeUsersController.Invite. Mirrors register/job-seeker: the invitee proves their own
    /// number with OtpPurpose.StaffInvite and sets their own password here.</summary>
    [HttpPost("accept-staff-invite")]
    public async Task<IActionResult> AcceptStaffInvite([FromBody] AcceptOfficeInviteDto dto, CancellationToken ct)
        => Respond(await service.AcceptStaffInviteAsync(dto, ct));

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
        => Respond(await service.LoginAsync(dto, ct));
}

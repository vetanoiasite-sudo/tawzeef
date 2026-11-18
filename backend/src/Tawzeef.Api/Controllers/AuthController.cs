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

    [HttpPost("register/job-seeker")]
    public async Task<IActionResult> RegisterJobSeeker([FromBody] RegisterJobSeekerDto dto, CancellationToken ct)
        => Respond(await service.RegisterJobSeekerAsync(dto, ct));

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
        => Respond(await service.LoginAsync(dto, ct));
}

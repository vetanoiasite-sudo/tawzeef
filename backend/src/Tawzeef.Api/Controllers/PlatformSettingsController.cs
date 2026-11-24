using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.PlatformSettings;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Api.Controllers;

/// <summary>SuperAdmin-only (user.IsSuperAdmin short-circuits every permission check) — see
/// ControllerModuleMap for the naming-convention resolution (Get -&gt; view, Update -&gt; edit).
/// GetSupportedCurrencies is the one exception, [AllowAnonymous] on that single action: any
/// signed-in role (office staff confirming a payment, not just SuperAdmin) needs the currency
/// list, and it isn't sensitive — same reasoning as PublicJobsController's categories/cities.</summary>
[Route("api/[controller]")]
public class PlatformSettingsController(IPlatformSettingsService service, ICurrentUser currentUser) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPlatformSettings(CancellationToken ct)
        => Respond(await service.ListAsync(ct));

    /// <summary>Replaces the frontend's old hardcoded SUPPORTED_CURRENCIES = ['EGP'] array — a
    /// currency dropdown always reflects the real platform.supported_currencies setting.</summary>
    [HttpGet("currencies")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSupportedCurrencies(CancellationToken ct)
        => Respond(await service.GetSupportedCurrenciesAsync(ct));

    [HttpPut("{key}")]
    public async Task<IActionResult> UpdatePlatformSetting(string key, [FromBody] UpdatePlatformSettingDto dto, CancellationToken ct)
        => Respond(await service.UpdateAsync(key, RequireUserId(), dto, ct));

    private Guid RequireUserId() => currentUser.Id ?? throw new UnauthorizedAccessException();
}

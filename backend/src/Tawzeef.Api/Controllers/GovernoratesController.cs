using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.Governorates;

namespace Tawzeef.Api.Controllers;

/// <summary>Anonymous (same reasoning as PublicJobsController.GetCategories) — every "pick a
/// city/governorate" field across the app (public job search, job ads, employer/job-seeker/office
/// profile forms) reads this instead of a frontend-hardcoded list.</summary>
[Route("api/[controller]")]
[AllowAnonymous]
public class GovernoratesController(IGovernorateService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
        => Respond(await service.ListAsync(ct));
}

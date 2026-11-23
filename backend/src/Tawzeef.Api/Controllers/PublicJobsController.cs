using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.JobAdvertisements;
using Tawzeef.Application.Features.PublicJobs;

namespace Tawzeef.Api.Controllers;

/// <summary>Anonymous, no-login public job board — API_DOCUMENTATION.md §3.7, the Wuzzuf-style
/// surface. [AllowAnonymous] on the class skips both the permission filter and the startup audit,
/// same pattern as AuthController.</summary>
[Route("api/jobs")]
[AllowAnonymous]
public class PublicJobsController(IPublicJobSearchService service, IJobAdvertisementReviewService jobAdvertisementService) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] JobSearchQueryDto query, CancellationToken ct)
        => RespondPaged(await service.SearchAsync(query, ct));

    /// <summary>Platform-wide categories (office-managed — JobCategory's doc comment), for the
    /// search filter dropdown. Not office-scoped, safe for anonymous use — not sensitive.</summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
        => Respond(await jobAdvertisementService.ListCategoriesAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Respond(await service.GetByIdAsync(id, ct));

    /// <summary>Home page "trust strip" — real platform counts, no fabricated numbers.</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct)
        => Respond(await service.GetPlatformStatsAsync(ct));
}

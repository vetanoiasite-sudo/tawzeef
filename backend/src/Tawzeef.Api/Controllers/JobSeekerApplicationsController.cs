using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Authorization;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.JobApplications;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Api.Controllers;

/// <summary>Job Seeker self-service — API_DOCUMENTATION.md §3.8's /api/job-seekers/me/applications routes.</summary>
[Route("api/job-seekers/me/applications")]
public class JobSeekerApplicationsController(IJobApplicationService service, ICurrentUser currentUser) : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateJobApplicationDto dto, CancellationToken ct)
        => Respond(await service.CreateAsync(RequireUserId(), dto, ct));

    [HttpGet]
    [RequiresPermission("jobapplications.viewown")]
    public async Task<IActionResult> List([FromQuery] JobApplicationQueryDto query, CancellationToken ct)
        => RespondPaged(await service.ListMineAsync(RequireUserId(), query, ct));

    [HttpPost("{id:guid}/withdraw")]
    [RequiresPermission("jobapplications.withdraw")]
    public async Task<IActionResult> Withdraw(Guid id, CancellationToken ct)
        => Respond(await service.WithdrawAsync(RequireUserId(), id, ct));

    private Guid RequireUserId() => currentUser.Id ?? throw new UnauthorizedAccessException();
}

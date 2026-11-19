using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Authorization;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.JobAdvertisements;
using Tawzeef.Application.Features.JobApplications;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Api.Controllers;

/// <summary>Employer self-service — API_DOCUMENTATION.md §3.6/§3.8's /api/employer/job-advertisements routes.</summary>
[Route("api/employer/job-advertisements")]
public class EmployerJobAdvertisementsController(
    IJobAdvertisementService service, IApplicantReviewService applicantReviewService, ICurrentUser currentUser) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] JobAdvertisementQueryDto query, CancellationToken ct)
        => RespondPaged(await service.ListMineAsync(RequireUserId(), query, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Respond(await service.GetMineByIdAsync(RequireUserId(), id, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateJobAdvertisementDto dto, CancellationToken ct)
        => Respond(await service.CreateAsync(RequireUserId(), dto, ct));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateJobAdvertisementDto dto, CancellationToken ct)
        => Respond(await service.UpdateAsync(RequireUserId(), id, dto, ct));

    [HttpPost("{id:guid}/submit")]
    [RequiresPermission("jobadvertisements.submit")]
    public async Task<IActionResult> SubmitForReview(Guid id, CancellationToken ct)
        => Respond(await service.SubmitForReviewAsync(RequireUserId(), id, ct));

    [HttpPost("{id:guid}/close")]
    [RequiresPermission("jobadvertisements.close")]
    public async Task<IActionResult> Close(Guid id, CancellationToken ct)
        => Respond(await service.CloseAsync(RequireUserId(), id, ct));

    [HttpPost("{id:guid}/renew")]
    [RequiresPermission("jobadvertisements.renew")]
    public async Task<IActionResult> Renew(Guid id, CancellationToken ct)
        => Respond(await service.RenewAsync(RequireUserId(), id, ct));

    [HttpGet("{id:guid}/applicants")]
    [RequiresPermission("jobapplications.view")]
    public async Task<IActionResult> ListApplicants(Guid id, [FromQuery] JobApplicationQueryDto query, CancellationToken ct)
        => RespondPaged(await applicantReviewService.ListForEmployerAsync(RequireUserId(), id, query, ct));

    private Guid RequireUserId() => currentUser.Id ?? throw new UnauthorizedAccessException();
}

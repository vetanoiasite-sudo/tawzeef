using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Authorization;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.Interviews;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Api.Controllers;

/// <summary>API_DOCUMENTATION.md §3.9's shared /api/applications/{id}/... routes — scoped per
/// caller (Employer own ad / Office own office / JobSeeker own application) in the service layer,
/// never by the controller's route prefix, since the same endpoints serve all three actors.</summary>
[Route("api/applications")]
public class ApplicationsController(
    IInterviewService interviewService,
    IHiringFollowUpService hiringFollowUpService,
    IApplicantEvaluationService evaluationService,
    ICurrentUser currentUser) : ApiControllerBase
{
    [HttpPost("{id:guid}/interview")]
    [RequiresPermission("interviews.schedule")]
    public async Task<IActionResult> ScheduleInterview(Guid id, [FromBody] CreateInterviewDto dto, CancellationToken ct)
        => Respond(await interviewService.ScheduleAsync(RequireUserId(), id, dto, ct));

    [HttpPut("{id:guid}/interview")]
    [RequiresPermission("interviews.edit")]
    public async Task<IActionResult> UpdateInterview(Guid id, [FromBody] UpdateInterviewDto dto, CancellationToken ct)
        => Respond(await interviewService.UpdateAsync(RequireUserId(), id, dto, ct));

    [HttpGet("{id:guid}/interview")]
    [RequiresPermission("interviews.view")]
    public async Task<IActionResult> GetInterview(Guid id, CancellationToken ct)
        => Respond(await interviewService.GetAsync(RequireUserId(), currentUser.OfficeId, id, ct));

    [HttpPost("{id:guid}/interview-report")]
    [RequiresPermission("interviewreports.create")]
    public async Task<IActionResult> CreateInterviewReport(Guid id, [FromBody] CreateInterviewReportDto dto, CancellationToken ct)
        => Respond(await interviewService.CreateReportAsync(RequireUserId(), id, dto, ct));

    [HttpGet("{id:guid}/interview-report")]
    [RequiresPermission("interviewreports.view")]
    public async Task<IActionResult> GetInterviewReport(Guid id, CancellationToken ct)
        => Respond(await interviewService.GetReportAsync(RequireUserId(), currentUser.OfficeId, id, ct));

    [HttpPost("{id:guid}/hiring-follow-up")]
    [RequiresPermission("hiringfollowupreports.create")]
    public async Task<IActionResult> CreateHiringFollowUp(Guid id, [FromBody] CreateHiringFollowUpDto dto, CancellationToken ct)
        => Respond(await hiringFollowUpService.CreateAsync(RequireOfficeId(), RequireUserId(), id, dto, ct));

    [HttpPut("{id:guid}/hiring-follow-up")]
    [RequiresPermission("hiringfollowupreports.create")]
    public async Task<IActionResult> CompleteHiringFollowUp(Guid id, [FromBody] CompleteHiringFollowUpDto dto, CancellationToken ct)
        => Respond(await hiringFollowUpService.CompleteAsync(RequireOfficeId(), RequireUserId(), id, dto, ct));

    [HttpGet("{id:guid}/hiring-follow-up")]
    [RequiresPermission("hiringfollowupreports.view")]
    public async Task<IActionResult> GetHiringFollowUp(Guid id, CancellationToken ct)
        => Respond(await hiringFollowUpService.GetAsync(RequireUserId(), currentUser.OfficeId, id, ct));

    [HttpPost("{id:guid}/evaluation")]
    [RequiresPermission("applicantevaluationreports.create")]
    public async Task<IActionResult> CreateEvaluation(Guid id, [FromBody] CreateApplicantEvaluationDto dto, CancellationToken ct)
        => Respond(await evaluationService.CreateOrReplaceAsync(RequireUserId(), id, dto, ct));

    [HttpGet("{id:guid}/evaluation")]
    [RequiresPermission("applicantevaluationreports.view")]
    public async Task<IActionResult> GetEvaluation(Guid id, CancellationToken ct)
        => Respond(await evaluationService.GetAsync(RequireUserId(), currentUser.OfficeId, id, ct));

    private Guid RequireUserId() => currentUser.Id ?? throw new UnauthorizedAccessException();
    private Guid RequireOfficeId() => currentUser.OfficeId ?? throw new UnauthorizedAccessException();
}

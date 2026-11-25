using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Authorization;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.JobAdvertisements;
using Tawzeef.Application.Features.JobApplications;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Api.Controllers;

/// <summary>Office review queue + manual payment confirmation — API_DOCUMENTATION.md §3.6/§3.8's
/// /api/office/job-advertisements routes.</summary>
[Route("api/office/job-advertisements")]
public class OfficeJobAdvertisementsController(
    IJobAdvertisementReviewService service, IApplicantReviewService applicantReviewService, ICurrentUser currentUser) : ApiControllerBase
{
    [HttpGet]
    [RequiresPermission("jobadvertisements.review")]
    public async Task<IActionResult> ListQueue([FromQuery] JobAdvertisementQueryDto query, CancellationToken ct)
        => RespondPaged(await service.ListQueueAsync(RequireOfficeId(), query, ct));

    [HttpGet("{id:guid}")]
    [RequiresPermission("jobadvertisements.review")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Respond(await service.GetByIdAsync(RequireOfficeId(), id, ct));

    /// <summary>Office-side correction — fixes a typo the employer themselves got wrong, no
    /// re-approval needed (IJobAdvertisementReviewService.UpdateAsync's doc comment).</summary>
    [HttpPut("{id:guid}")]
    [RequiresPermission("jobadvertisements.edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateJobAdvertisementDto dto, CancellationToken ct)
        => Respond(await service.UpdateAsync(RequireOfficeId(), id, dto, ct));

    /// <summary>Office-only category assignment — required before Approve will succeed
    /// (IJobAdvertisementReviewService.AssignCategoryAsync's doc comment).</summary>
    [HttpPost("{id:guid}/category")]
    [RequiresPermission("jobadvertisements.approve")]
    public async Task<IActionResult> AssignCategory(Guid id, [FromBody] AssignJobCategoryDto dto, CancellationToken ct)
        => Respond(await service.AssignCategoryAsync(RequireOfficeId(), id, dto, ct));

    [HttpPost("{id:guid}/approve")]
    [RequiresPermission("jobadvertisements.approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
        => Respond(await service.ApproveAsync(RequireOfficeId(), RequireUserId(), id, ct));

    [HttpPost("{id:guid}/reject")]
    [RequiresPermission("jobadvertisements.reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] JobAdvertisementReasonDto dto, CancellationToken ct)
        => Respond(await service.RejectAsync(RequireOfficeId(), RequireUserId(), id, dto.Reason, ct));

    [HttpPost("{id:guid}/pause")]
    [RequiresPermission("jobadvertisements.pause")]
    public async Task<IActionResult> Pause(Guid id, CancellationToken ct)
        => Respond(await service.PauseAsync(RequireOfficeId(), id, ct));

    [HttpPost("{id:guid}/resume")]
    [RequiresPermission("jobadvertisements.pause")]
    public async Task<IActionResult> Resume(Guid id, CancellationToken ct)
        => Respond(await service.ResumeAsync(RequireOfficeId(), id, ct));

    [HttpPost("{id:guid}/payment/confirm")]
    [RequiresPermission("advertisementpayments.record")]
    public async Task<IActionResult> ConfirmPayment(Guid id, [FromForm] ConfirmAdvertisementPaymentRequest request, CancellationToken ct)
    {
        if (request.File is null || request.File.Length == 0)
            return BadRequest(new { message = "صورة إيصال الدفع مطلوبة." });

        var dto = new ConfirmAdvertisementPaymentDto { Amount = request.Amount, Currency = request.Currency };
        await using var stream = request.File.OpenReadStream();
        return Respond(await service.ConfirmPaymentAsync(
            RequireOfficeId(), RequireUserId(), id, dto,
            stream, request.File.FileName, request.File.ContentType, request.File.Length, ct));
    }

    [HttpGet("{id:guid}/applicants")]
    [RequiresPermission("jobapplications.view")]
    public async Task<IActionResult> ListApplicants(Guid id, [FromQuery] JobApplicationQueryDto query, CancellationToken ct)
        => RespondPaged(await applicantReviewService.ListForOfficeAsync(RequireOfficeId(), id, query, ct));

    private Guid RequireUserId() => currentUser.Id ?? throw new UnauthorizedAccessException();
    private Guid RequireOfficeId() => currentUser.OfficeId ?? throw new UnauthorizedAccessException();
}

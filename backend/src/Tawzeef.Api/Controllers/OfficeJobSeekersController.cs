using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Authorization;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.JobSeekers;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Api.Controllers;

/// <summary>The office review queue — RBAC_PERMISSIONS.md §3's plain jobseekers.view/.review/
/// .approve/.reject codes, office-scoped. Not the self-service "...own" surface in
/// JobSeekersController. API_DOCUMENTATION.md's /api/office/job-seekers routes.</summary>
[Route("api/office/job-seekers")]
public class OfficeJobSeekersController(IJobSeekerReviewService service, ICurrentUser currentUser) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListQueue([FromQuery] JobSeekerReviewQueryDto query, CancellationToken ct)
        => RespondPaged(await service.ListQueueAsync(RequireOfficeId(), query, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Respond(await service.GetByIdAsync(RequireOfficeId(), id, ct));

    /// <summary>Not a signed, short-lived URL token (no such file-serving infrastructure exists
    /// yet) — an ordinary authenticated GET, re-checking office ownership on every call, streamed
    /// through the existing IFileStorage abstraction. Mirrors
    /// EmployerApplicationsController.DownloadCv's exact pattern.</summary>
    [HttpGet("{id:guid}/documents/{documentId:guid}/download")]
    [RequiresPermission("jobseekers.viewdocuments")]
    public async Task<IActionResult> DownloadDocument(Guid id, Guid documentId, CancellationToken ct)
    {
        var result = await service.GetDocumentFileAsync(RequireOfficeId(), id, documentId, ct);
        if (!result.IsSuccess) return Respond(result);

        var file = result.Data!;
        return File(file.Content, string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType, file.FileName);
    }

    [HttpPost("{id:guid}/documents/{documentId:guid}/approve")]
    [RequiresPermission("jobseekers.review")]
    public async Task<IActionResult> ApproveDocument(Guid id, Guid documentId, CancellationToken ct)
        => Respond(await service.ApproveDocumentAsync(RequireOfficeId(), RequireUserId(), id, documentId, ct));

    [HttpPost("{id:guid}/documents/{documentId:guid}/reject")]
    [RequiresPermission("jobseekers.review")]
    public async Task<IActionResult> RejectDocument(Guid id, Guid documentId, [FromBody] ReasonDto dto, CancellationToken ct)
        => Respond(await service.RejectDocumentAsync(RequireOfficeId(), RequireUserId(), id, documentId, dto.Reason, ct));

    [HttpPost("{id:guid}/approve")]
    [RequiresPermission("jobseekers.approve")]
    public async Task<IActionResult> ApproveProfile(Guid id, CancellationToken ct)
        => Respond(await service.ApproveProfileAsync(RequireOfficeId(), RequireUserId(), id, ct));

    [HttpPost("{id:guid}/reject")]
    [RequiresPermission("jobseekers.reject")]
    public async Task<IActionResult> RejectProfile(Guid id, [FromBody] ReasonDto dto, CancellationToken ct)
        => Respond(await service.RejectProfileAsync(RequireOfficeId(), RequireUserId(), id, dto.Reason, ct));

    /// <summary>OfficeAdmin only — RBAC_PERMISSIONS.md §0 item 8 / footnote 5, enforced by
    /// PermissionCatalogue.OfficeAdminOnly in PermissionChecker, not by this attribute alone.</summary>
    [HttpPost("{id:guid}/suspend")]
    [RequiresPermission("jobseekers.suspend")]
    public async Task<IActionResult> Suspend(Guid id, [FromBody] ReasonDto dto, CancellationToken ct)
        => Respond(await service.SuspendAsync(RequireOfficeId(), RequireUserId(), id, dto.Reason, ct));

    [HttpPost("{id:guid}/reinstate")]
    [RequiresPermission("jobseekers.reinstate")]
    public async Task<IActionResult> Reinstate(Guid id, [FromBody] ReasonDto dto, CancellationToken ct)
        => Respond(await service.ReinstateAsync(RequireOfficeId(), RequireUserId(), id, dto.Reason, ct));

    private Guid RequireUserId() => currentUser.Id ?? throw new UnauthorizedAccessException();
    private Guid RequireOfficeId() => currentUser.OfficeId ?? throw new UnauthorizedAccessException();
}

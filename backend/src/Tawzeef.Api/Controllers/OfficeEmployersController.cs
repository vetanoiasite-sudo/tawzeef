using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Authorization;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.Employers;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Api.Controllers;

/// <summary>The office review queue — mirrors OfficeJobSeekersController exactly, plus
/// stop/resume-cooperation (WORKFLOWS_AND_STATE_MACHINES.md §4, independent of
/// VerificationStatus). API_DOCUMENTATION.md §3.5's /api/office/employers routes.</summary>
[Route("api/office/employers")]
public class OfficeEmployersController(IEmployerReviewService service, ICurrentUser currentUser) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListQueue([FromQuery] EmployerReviewQueryDto query, CancellationToken ct)
        => RespondPaged(await service.ListQueueAsync(RequireOfficeId(), query, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Respond(await service.GetByIdAsync(RequireOfficeId(), id, ct));

    /// <summary>Not a signed, short-lived URL token (no such file-serving infrastructure exists
    /// yet) — an ordinary authenticated GET, re-checking office ownership on every call, streamed
    /// through the existing IFileStorage abstraction. Mirrors OfficeJobSeekersController's exact
    /// pattern.</summary>
    [HttpGet("{id:guid}/documents/{documentId:guid}/download")]
    [RequiresPermission("employers.viewdocuments")]
    public async Task<IActionResult> DownloadDocument(Guid id, Guid documentId, CancellationToken ct)
    {
        var result = await service.GetDocumentFileAsync(RequireOfficeId(), id, documentId, ct);
        if (!result.IsSuccess) return Respond(result);

        var file = result.Data!;
        return File(file.Content, string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType, file.FileName);
    }

    [HttpPost("{id:guid}/documents/{documentId:guid}/approve")]
    [RequiresPermission("employers.review")]
    public async Task<IActionResult> ApproveDocument(Guid id, Guid documentId, CancellationToken ct)
        => Respond(await service.ApproveDocumentAsync(RequireOfficeId(), RequireUserId(), id, documentId, ct));

    [HttpPost("{id:guid}/documents/{documentId:guid}/reject")]
    [RequiresPermission("employers.review")]
    public async Task<IActionResult> RejectDocument(Guid id, Guid documentId, [FromBody] EmployerReasonDto dto, CancellationToken ct)
        => Respond(await service.RejectDocumentAsync(RequireOfficeId(), RequireUserId(), id, documentId, dto.Reason, ct));

    [HttpPost("{id:guid}/approve")]
    [RequiresPermission("employers.approve")]
    public async Task<IActionResult> ApproveProfile(Guid id, CancellationToken ct)
        => Respond(await service.ApproveProfileAsync(RequireOfficeId(), RequireUserId(), id, ct));

    [HttpPost("{id:guid}/reject")]
    [RequiresPermission("employers.reject")]
    public async Task<IActionResult> RejectProfile(Guid id, [FromBody] EmployerReasonDto dto, CancellationToken ct)
        => Respond(await service.RejectProfileAsync(RequireOfficeId(), RequireUserId(), id, dto.Reason, ct));

    /// <summary>API_DOCUMENTATION.md §3.5: gated by complaints.decide (RBAC_PERMISSIONS.md
    /// footnote 5), not employers.suspend — OfficeAdmin only, same PermissionCatalogue.OfficeAdminOnly
    /// enforcement as everything else under that footnote.</summary>
    [HttpPost("{id:guid}/stop-cooperation")]
    [RequiresPermission("complaints.decide")]
    public async Task<IActionResult> StopCooperation(Guid id, [FromBody] EmployerReasonDto dto, CancellationToken ct)
        => Respond(await service.StopCooperationAsync(RequireOfficeId(), RequireUserId(), id, dto.Reason, ct));

    [HttpPost("{id:guid}/resume-cooperation")]
    [RequiresPermission("complaints.decide")]
    public async Task<IActionResult> ResumeCooperation(Guid id, [FromBody] EmployerReasonDto dto, CancellationToken ct)
        => Respond(await service.ResumeCooperationAsync(RequireOfficeId(), RequireUserId(), id, dto.Reason, ct));

    private Guid RequireUserId() => currentUser.Id ?? throw new UnauthorizedAccessException();
    private Guid RequireOfficeId() => currentUser.OfficeId ?? throw new UnauthorizedAccessException();
}

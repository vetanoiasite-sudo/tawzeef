using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Authorization;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.Courses;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Api.Controllers;

/// <summary>Office-side course management — OfficeAdmin only (PermissionCatalogue.OfficeAdminOnly),
/// no review workflow: the office publishes directly.</summary>
[Route("api/office/courses")]
public class OfficeCoursesController(ICourseService service, ICurrentUser currentUser) : ApiControllerBase
{
    [HttpGet]
    [RequiresPermission("courses.view")]
    public async Task<IActionResult> List([FromQuery] CourseQueryDto query, CancellationToken ct)
        => RespondPaged(await service.ListForOfficeAsync(RequireOfficeId(), query, ct));

    [HttpGet("{id:guid}")]
    [RequiresPermission("courses.view")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Respond(await service.GetByIdForOfficeAsync(RequireOfficeId(), id, ct));

    [HttpPost]
    [RequiresPermission("courses.create")]
    public async Task<IActionResult> Create([FromBody] CreateCourseDto dto, CancellationToken ct)
        => Respond(await service.CreateAsync(RequireOfficeId(), RequireUserId(), dto, ct));

    [HttpPut("{id:guid}")]
    [RequiresPermission("courses.edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseDto dto, CancellationToken ct)
        => Respond(await service.UpdateAsync(RequireOfficeId(), id, dto, ct));

    [HttpPost("{id:guid}/image")]
    [RequiresPermission("courses.edit")]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "الملف مطلوب." });

        await using var stream = file.OpenReadStream();
        return Respond(await service.UploadImageAsync(RequireOfficeId(), id, stream, file.FileName, file.ContentType, file.Length, ct));
    }

    [HttpPost("{id:guid}/pause")]
    [RequiresPermission("courses.pause")]
    public async Task<IActionResult> Pause(Guid id, CancellationToken ct)
        => Respond(await service.PauseAsync(RequireOfficeId(), id, ct));

    [HttpPost("{id:guid}/resume")]
    [RequiresPermission("courses.pause")]
    public async Task<IActionResult> Resume(Guid id, CancellationToken ct)
        => Respond(await service.ResumeAsync(RequireOfficeId(), id, ct));

    private Guid RequireUserId() => currentUser.Id ?? throw new UnauthorizedAccessException();
    private Guid RequireOfficeId() => currentUser.OfficeId ?? throw new UnauthorizedAccessException();
}

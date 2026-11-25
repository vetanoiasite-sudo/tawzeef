using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Authorization;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.Offices;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Api.Controllers;

[Route("api/[controller]")]
public class OfficesController(IOfficeService service, ICurrentUser currentUser) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetOffices(CancellationToken ct)
        => Respond(await service.ListAsync(ct));

    [HttpPost]
    public async Task<IActionResult> CreateOffice([FromBody] CreateOfficeDto dto, CancellationToken ct)
        => Respond(await service.CreateAsync(dto, ct));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateOffice(Guid id, [FromBody] UpdateOfficeDto dto, CancellationToken ct)
        => Respond(await service.UpdateAsync(id, dto, ct));

    [HttpPost("{id:guid}/activate")]
    [RequiresPermission("offices.edit")]
    public async Task<IActionResult> ActivateOffice(Guid id, CancellationToken ct)
        => Respond(await service.ActivateAsync(id, ct));

    [HttpPost("{id:guid}/deactivate")]
    [RequiresPermission("offices.deactivate")]
    public async Task<IActionResult> DeactivateOffice(Guid id, CancellationToken ct)
        => Respond(await service.DeactivateAsync(id, ct));

    /// <summary>SuperAdmin dashboard tiles — cheap counts, no new entities.</summary>
    [HttpGet("stats")]
    [RequiresPermission("offices.view")]
    public async Task<IActionResult> GetStats(CancellationToken ct)
        => Respond(await service.GetStatsAsync(ct));

    /// <summary>Public registration dropdown — name/city only (RBAC_PERMISSIONS.md §0 item 3).</summary>
    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicOffices(CancellationToken ct)
        => Respond(await service.ListPublicAsync(ct));

    /// <summary>The office's own self-service "my office" profile page (2026-11-25) — mirrors
    /// JobSeekersController/EmployersController's "me" pattern. OfficeAdmin gets every permission
    /// for their own office automatically (PermissionChecker); OfficeStaff needs an explicit
    /// offices.view/offices.edit StaffPermissionGrant, same as any other staff-grantable action.</summary>
    [HttpGet("me")]
    [RequiresPermission("offices.view")]
    public async Task<IActionResult> GetMyOffice(CancellationToken ct)
        => Respond(await service.GetByIdAsync(RequireOfficeId(), ct));

    [HttpPut("me")]
    [RequiresPermission("offices.edit")]
    public async Task<IActionResult> UpdateMyOffice([FromBody] UpdateOfficeDto dto, CancellationToken ct)
        => Respond(await service.UpdateAsync(RequireOfficeId(), dto, ct));

    [HttpPost("me/logo")]
    [RequiresPermission("offices.edit")]
    public async Task<IActionResult> UploadMyLogo(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "الملف مطلوب." });

        await using var stream = file.OpenReadStream();
        return Respond(await service.UploadLogoAsync(RequireOfficeId(), stream, file.FileName, file.ContentType, file.Length, ct));
    }

    /// <summary>The public "view this office's profile" page — job seekers/employers browsing
    /// offices before (or instead of) the registration dropdown. Same anonymity rule as a paused
    /// job ad: an Inactive office's id confirms nothing (API_DOCUMENTATION.md §3.7).</summary>
    [HttpGet("{id:guid}/public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicOffice(Guid id, CancellationToken ct)
        => Respond(await service.GetPublicByIdAsync(id, ct));

    /// <summary>The one PDF/Excel-style report this app needed concretely scoped: a CSV (opens
    /// directly in Excel) of the office directory, for SuperAdmin to hand off or archive. A
    /// plain controller-level CSV writer, no new dependency — matches this codebase's
    /// dependency-light style.</summary>
    [HttpGet("export")]
    [RequiresPermission("offices.view")]
    public async Task<IActionResult> GetOfficesExport(CancellationToken ct)
    {
        var result = await service.ListAsync(ct);
        if (!result.IsSuccess) return Respond(result);

        var csv = new StringBuilder();
        csv.AppendLine("Name,ArabicName,Code,City,Address,Phone,Email,Status");
        foreach (var office in result.Data!)
        {
            csv.AppendLine(string.Join(',', new[]
            {
                office.Name, office.ArName, office.Code, office.City, office.Address, office.Phone, office.Email, office.Status,
            }.Select(CsvEscape)));
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
        return File(bytes, "text/csv", $"offices-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private static string CsvEscape(string? value)
    {
        value ??= string.Empty;
        return value.Contains(',') || value.Contains('"') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
    }

    /// <summary>No review workflow — a logo upload just replaces the previous one. Reuses
    /// offices.edit, same as every other SuperAdmin office edit.</summary>
    [HttpPost("{id:guid}/logo")]
    [RequiresPermission("offices.edit")]
    public async Task<IActionResult> UploadLogo(Guid id, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "A file is required." });

        await using var stream = file.OpenReadStream();
        return Respond(await service.UploadLogoAsync(id, stream, file.FileName, file.ContentType, file.Length, ct));
    }

    /// <summary>The one file this app serves to an anonymous caller for an office — a logo is
    /// meant for public display, not sensitive like everything else (EmploymentOffice.LogoStorageKey's
    /// doc comment).</summary>
    [HttpGet("{id:guid}/logo")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLogo(Guid id, CancellationToken ct)
    {
        var logo = await service.GetLogoFileAsync(id, ct);
        if (logo is null) return NotFound();

        return File(logo.Content, logo.ContentType);
    }

    private Guid RequireOfficeId() => currentUser.OfficeId ?? throw new UnauthorizedAccessException();
}

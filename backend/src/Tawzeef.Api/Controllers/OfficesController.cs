using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Authorization;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.Offices;

namespace Tawzeef.Api.Controllers;

[Route("api/[controller]")]
public class OfficesController(IOfficeService service) : ApiControllerBase
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
}

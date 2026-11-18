using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    /// <summary>Public registration dropdown — name/city only (RBAC_PERMISSIONS.md §0 item 3).</summary>
    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicOffices(CancellationToken ct)
        => Respond(await service.ListPublicAsync(ct));
}

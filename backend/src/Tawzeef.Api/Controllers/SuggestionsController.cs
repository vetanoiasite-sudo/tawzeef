using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Authorization;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.Suggestions;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Api.Controllers;

/// <summary>Self-service — a JobSeeker or an Employer submitting/viewing their own suggestions
/// (RBAC_PERMISSIONS.md §3: suggestions.create/.viewown, granted to either role simply by holding
/// it). Not the office review surface (OfficeSuggestionsController).</summary>
[Route("api/suggestions")]
public class SuggestionsController(ISuggestionService service, ICurrentUser currentUser) : ApiControllerBase
{
    [HttpPost]
    [RequiresPermission("suggestions.create")]
    public async Task<IActionResult> Create([FromBody] CreateSuggestionDto dto, CancellationToken ct)
        => Respond(await service.CreateAsync(RequireUserId(), dto, ct));

    [HttpGet("mine")]
    [RequiresPermission("suggestions.viewown")]
    public async Task<IActionResult> ListMine([FromQuery] int pageIndex, [FromQuery] int pageSize, CancellationToken ct)
        => RespondPaged(await service.ListMineAsync(RequireUserId(), pageIndex, pageSize, ct));

    private Guid RequireUserId() => currentUser.Id ?? throw new UnauthorizedAccessException();
}

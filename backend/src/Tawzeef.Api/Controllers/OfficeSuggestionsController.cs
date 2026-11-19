using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Authorization;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.Suggestions;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Api.Controllers;

/// <summary>The office suggestion-review queue — RBAC_PERMISSIONS.md §3's
/// suggestions.view/.respond, office-scoped. API_DOCUMENTATION.md §3.10.</summary>
[Route("api/office/suggestions")]
public class OfficeSuggestionsController(ISuggestionReviewService service, ICurrentUser currentUser) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListQueue([FromQuery] SuggestionQueryDto query, CancellationToken ct)
        => RespondPaged(await service.ListQueueAsync(RequireOfficeId(), query, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Respond(await service.GetByIdAsync(RequireOfficeId(), id, ct));

    [HttpPost("{id:guid}/respond")]
    [RequiresPermission("suggestions.respond")]
    public async Task<IActionResult> RespondToSuggestion(Guid id, [FromBody] RespondSuggestionDto dto, CancellationToken ct)
        => Respond(await service.RespondAsync(RequireOfficeId(), id, dto.OfficeNotes, ct));

    private Guid RequireOfficeId() => currentUser.OfficeId ?? throw new UnauthorizedAccessException();
}

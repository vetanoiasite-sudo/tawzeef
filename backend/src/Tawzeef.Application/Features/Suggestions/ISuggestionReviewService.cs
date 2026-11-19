using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;

namespace Tawzeef.Application.Features.Suggestions;

/// <summary>Office review — API_DOCUMENTATION.md §3.10's /api/office/suggestions routes.
/// GetByIdAsync isn't in that route table, but is added as a convenience the same way the
/// hiring-follow-up single GET was — Suggestion.Content can run to 4000 chars, impractical to show
/// in a list row.</summary>
public interface ISuggestionReviewService
{
    Task<ServiceResult<PagedResult<SuggestionDto>>> ListQueueAsync(Guid officeId, SuggestionQueryDto query, CancellationToken ct = default);
    Task<ServiceResult<SuggestionDto>> GetByIdAsync(Guid officeId, Guid id, CancellationToken ct = default);
    Task<ServiceResult<SuggestionDto>> RespondAsync(Guid officeId, Guid id, string officeNotes, CancellationToken ct = default);
}

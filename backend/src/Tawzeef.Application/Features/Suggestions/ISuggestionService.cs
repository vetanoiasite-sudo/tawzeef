using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;

namespace Tawzeef.Application.Features.Suggestions;

/// <summary>Self-service — API_DOCUMENTATION.md §3.10's /api/suggestions routes.</summary>
public interface ISuggestionService
{
    Task<ServiceResult<SuggestionDto>> CreateAsync(Guid userId, CreateSuggestionDto dto, CancellationToken ct = default);
    Task<ServiceResult<PagedResult<SuggestionDto>>> ListMineAsync(Guid userId, int pageIndex, int pageSize, CancellationToken ct = default);
}

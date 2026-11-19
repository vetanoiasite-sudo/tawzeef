using Tawzeef.Domain.Entities.Suggestions;

namespace Tawzeef.Application.Features.Suggestions;

internal static class SuggestionMapping
{
    public static SuggestionDto ToDto(this Suggestion s) => new()
    {
        Id = s.Id,
        Content = s.Content,
        Status = s.Status.ToString(),
        OfficeNotes = s.OfficeNotes,
        CreationDate = s.CreationDate,
    };
}

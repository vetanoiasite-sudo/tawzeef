using System.ComponentModel.DataAnnotations;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.Suggestions;

public class CreateSuggestionDto
{
    [Required, MaxLength(4000)]
    public string Content { get; set; } = string.Empty;
}

public class RespondSuggestionDto
{
    [Required, MaxLength(2000)]
    public string OfficeNotes { get; set; } = string.Empty;
}

/// <summary>No separate detail route exists for suggestions (API_DOCUMENTATION.md §3.10's table
/// only lists the list + respond routes), so this one shape carries full Content both in "my
/// suggestions" and in the office queue/detail — unlike Complaint, which has a dedicated detail DTO.</summary>
public class SuggestionDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? OfficeNotes { get; set; }
    public DateTime CreationDate { get; set; }
}

public class SuggestionQueryDto
{
    public SuggestionStatus? Status { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; } = 20;
}

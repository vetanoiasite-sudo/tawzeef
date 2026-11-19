using Tawzeef.Domain.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.Suggestions;

/// <summary>DATABASE_DOCUMENTATION.md §7. Unlike Complaint, a suggestion has no polymorphic
/// subject — it's submitted TO the sender's own office, not about anyone. Anonymous submission is
/// not supported (same read of §11.1 as Complaint).</summary>
public class Suggestion : IEntity
{
    public Guid Id { get; set; }

    public Guid OfficeId { get; set; }
    public Guid SubmittedByUserId { get; set; }
    public ComplaintSenderType SenderType { get; set; }

    public string Content { get; set; } = string.Empty;
    public SuggestionStatus Status { get; set; } = SuggestionStatus.New;
    public string? OfficeNotes { get; set; }

    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;
}

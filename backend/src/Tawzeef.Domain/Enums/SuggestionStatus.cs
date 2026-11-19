namespace Tawzeef.Domain.Enums;

/// <summary>WORKFLOWS_AND_STATE_MACHINES.md §10.2. Closed has no documented endpoint that reaches
/// it in this slice — modeled for schema completeness, same deferral as ComplaintStatus.Closed.</summary>
public enum SuggestionStatus : byte
{
    New = 0,
    UnderReview = 1,
    Responded = 2,
    Closed = 3,
}

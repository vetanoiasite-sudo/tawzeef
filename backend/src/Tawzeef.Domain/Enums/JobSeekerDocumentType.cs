namespace Tawzeef.Domain.Enums;

/// <summary>DATABASE_DOCUMENTATION.md §4 — BUSINESS_REQUIREMENTS.md §5.1 Step 4, the 3 required
/// documents a Job Seeker must upload before submitting for review.</summary>
public enum JobSeekerDocumentType : byte
{
    RecommendationLetter = 0,
    NationalId = 1,
    Cv = 2,
}

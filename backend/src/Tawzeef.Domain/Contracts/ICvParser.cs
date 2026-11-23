namespace Tawzeef.Domain.Contracts;

/// <summary>Best-effort suggestions only, never persisted by the parser itself — the caller (see
/// JobSeekerProfileService.UploadDocumentAsync) returns these as hints on the upload response; the
/// job seeker reviews/edits them in the profile form and saves explicitly. Never a phone number or
/// email — the implementation excludes any line matching either pattern from every heuristic.</summary>
public record CvParseResult(string? SuggestedJobTitle, byte? SuggestedYearsOfExperience);

/// <summary>Local, regex/marker-heuristic text extraction — not AI/NLP, no network call, no cost
/// per upload (a deliberate product choice over an LLM-backed parser: lower accuracy on unusually
/// formatted CVs, but free, offline, and behaves identically in dev and production).</summary>
public interface ICvParser
{
    /// <returns>null if the content type isn't parseable (e.g. an image — no OCR in scope) or
    /// nothing useful was found.</returns>
    Task<CvParseResult?> TryParseAsync(byte[] fileBytes, string contentType, CancellationToken ct = default);
}

using System.Text.RegularExpressions;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Infrastructure.CvParsing;

/// <summary>Local, regex/marker-heuristic suggestions only — see ICvParser's doc comment and
/// PdfTextExtractor's for the deliberate scope limits (no AI, no OCR, PDF only).</summary>
public class CvParser : ICvParser
{
    private const string PdfContentType = "application/pdf";
    private const int MaxJobTitleLength = 150;

    private static readonly Regex EmailRegex = new(
        @"[\w.+-]+@[\w-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled);

    private static readonly Regex PhoneRegex = new(
        @"(01[0-9]{9})|(\+?\d[\d\-\s]{7,}\d)", RegexOptions.Compiled);

    private static readonly string[] JobTitleMarkers =
    [
        "المسمى الوظيفي", "الوظيفة الحالية", "المنصب",
        "job title", "current role", "position",
    ];

    private static readonly Regex YearsExperienceEnRegex = new(
        @"(\d{1,2})\s*\+?\s*years?\s*(?:of\s*)?experience", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex YearsExperienceArRegex1 = new(
        @"(\d{1,2})\s*سن(?:ة|وات)?\s*(?:من\s*)?خبرة", RegexOptions.Compiled);

    private static readonly Regex YearsExperienceArRegex2 = new(
        @"خبرة\s*(\d{1,2})\s*سن", RegexOptions.Compiled);

    public Task<CvParseResult?> TryParseAsync(byte[] fileBytes, string contentType, CancellationToken ct = default)
    {
        // No OCR in scope — an image CV (JPG/PNG, also allowed for this document type) simply
        // yields no suggestions; the job seeker fills the form by hand.
        if (!string.Equals(contentType, PdfContentType, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<CvParseResult?>(null);

        try
        {
            var text = PdfTextExtractor.ExtractText(fileBytes);
            return Task.FromResult(ParseText(text));
        }
        catch
        {
            // A malformed/corrupt/unusually-structured PDF must never break the upload itself.
            return Task.FromResult<CvParseResult?>(null);
        }
    }

    /// <summary>Public (not just internal) purely so the regex/exclusion heuristics can be unit
    /// tested directly on sample extracted text, without a cross-assembly InternalsVisibleTo —
    /// pure, stateless, no side effects.</summary>
    public static CvParseResult? ParseText(string text)
    {
        var lines = text
            .Split('\n')
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToList();

        var jobTitle = FindJobTitle(lines);
        var years = FindYearsOfExperience(text);

        return jobTitle is null && years is null ? null : new CvParseResult(jobTitle, years);
    }

    private static bool IsContactLine(string line) => EmailRegex.IsMatch(line) || PhoneRegex.IsMatch(line);

    /// <summary>Only returns a title when an explicit marker ("Job Title:", "المسمى الوظيفي", ...)
    /// is found — never guesses from an arbitrary line. A line matching the marker is skipped
    /// entirely if it also looks like a phone/email line, and the fallback "next line" search does
    /// the same, so a contact line can never surface as the suggested job title even if it happens
    /// to also contain a marker-like word.</summary>
    private static string? FindJobTitle(List<string> lines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (IsContactLine(line)) continue;

            var marker = JobTitleMarkers.FirstOrDefault(m => line.Contains(m, StringComparison.OrdinalIgnoreCase));
            if (marker is null) continue;

            var markerPos = line.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            var afterMarker = line[(markerPos + marker.Length)..].TrimStart(':', '-', '،', ' ').Trim();
            if (afterMarker.Length > 0 && !IsContactLine(afterMarker))
                return Truncate(afterMarker);

            for (var j = i + 1; j < lines.Count; j++)
            {
                if (IsContactLine(lines[j])) continue;
                return Truncate(lines[j]);
            }
        }
        return null;
    }

    private static byte? FindYearsOfExperience(string text)
    {
        foreach (var regex in new[] { YearsExperienceEnRegex, YearsExperienceArRegex1, YearsExperienceArRegex2 })
        {
            var match = regex.Match(text);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var years) && years is >= 0 and <= 60)
                return (byte)years;
        }
        return null;
    }

    private static string Truncate(string value) => value.Length <= MaxJobTitleLength ? value : value[..MaxJobTitleLength];
}

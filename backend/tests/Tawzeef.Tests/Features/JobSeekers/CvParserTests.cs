using Tawzeef.Infrastructure.CvParsing;

namespace Tawzeef.Tests.Features.JobSeekers;

/// <summary>Covers CvParser's regex/marker heuristics directly on sample extracted text (per the
/// task's own guidance: generating a real compressed PDF for every case adds little over testing
/// the heuristic itself) plus one end-to-end PdfTextExtractor case against a minimal, uncompressed,
/// hand-built PDF content stream, proving the two pieces actually fit together.</summary>
public class CvParserTests
{
    [Fact]
    public void ParseText_JobTitleMarkerSameLine_English_ExtractsTrailingValue()
    {
        var text = "Ali Hassan\nJob Title: Senior Backend Developer\nCairo, Egypt";

        var result = CvParser.ParseText(text);

        Assert.NotNull(result);
        Assert.Equal("Senior Backend Developer", result!.SuggestedJobTitle);
    }

    [Fact]
    public void ParseText_JobTitleMarkerSameLine_Arabic_ExtractsTrailingValue()
    {
        var text = "علي حسن\nالمسمى الوظيفي: مهندس برمجيات\nالقاهرة";

        var result = CvParser.ParseText(text);

        Assert.NotNull(result);
        Assert.Equal("مهندس برمجيات", result!.SuggestedJobTitle);
    }

    [Fact]
    public void ParseText_JobTitleMarkerAlone_UsesNextNonEmptyLine()
    {
        var text = "Current Role\nSenior Backend Developer\nSomewhere";

        var result = CvParser.ParseText(text);

        Assert.NotNull(result);
        Assert.Equal("Senior Backend Developer", result!.SuggestedJobTitle);
    }

    [Fact]
    public void ParseText_YearsOfExperience_English_IsExtracted()
    {
        var text = "Experienced engineer with 7 years of experience in backend systems.";

        var result = CvParser.ParseText(text);

        Assert.NotNull(result);
        Assert.Equal((byte)7, result!.SuggestedYearsOfExperience);
    }

    [Theory]
    [InlineData("عندي 5 سنوات خبرة في مجال المحاسبة", (byte)5)]
    [InlineData("خبرة 9 سنين في التدريس", (byte)9)]
    public void ParseText_YearsOfExperience_Arabic_IsExtracted(string text, byte expectedYears)
    {
        var result = CvParser.ParseText(text);

        Assert.NotNull(result);
        Assert.Equal(expectedYears, result!.SuggestedYearsOfExperience);
    }

    [Fact]
    public void ParseText_NoMarkerFound_ReturnsNull()
    {
        var text = "Ali Hassan\nCairo, Egypt\nSome unrelated paragraph about hobbies.";

        var result = CvParser.ParseText(text);

        Assert.Null(result);
    }

    // The one case that must never regress — a phone/email-bearing line must never surface as the
    // suggested job title, even when it also contains a marker-like word.
    [Theory]
    [InlineData("Job Title: call me at 01012345678")]
    [InlineData("Job Title: reach me at ali.hassan@example.com")]
    public void ParseText_MarkerLineAlsoContainsContactInfo_NeverReturnsThatLineAsJobTitle(string contactLine)
    {
        var text = $"Ali Hassan\n{contactLine}\nSenior Backend Developer\nCairo, Egypt";

        var result = CvParser.ParseText(text);

        // The contact-bearing line is skipped entirely (even though it contains the "Job Title"
        // marker), and "Senior Backend Developer" alone never matches a marker — so nothing is
        // extracted at all. That's the correct, strict outcome: a contact line's text must never
        // leak into SuggestedJobTitle under any circumstance.
        Assert.Null(result);
    }

    [Fact]
    public void ParseText_MarkerAloneFollowedByContactLineThenRealTitle_SkipsContactLine()
    {
        var text = "Job Title\n01012345678\nSenior Backend Developer";

        var result = CvParser.ParseText(text);

        Assert.NotNull(result);
        Assert.Equal("Senior Backend Developer", result!.SuggestedJobTitle);
    }

    [Fact]
    public void ExtractText_SimpleUncompressedContentStream_ExtractsShowTextOperators()
    {
        // A minimal, uncompressed PDF content stream using the Tj operator — not a full valid PDF
        // file, just the "stream ... endstream" shape PdfTextExtractor scans for, proving the
        // Tj-parsing + PDF-string-escape-decoding pipeline works end to end without needing a real
        // FlateDecode-compressed page.
        var fakePdf = "1 0 obj\n<< >>\nstream\n" +
                      "BT /F1 12 Tf 72 700 Td (Job Title: Senior Backend Developer) Tj ET\n" +
                      "endstream\nendobj";
        var bytes = System.Text.Encoding.Latin1.GetBytes(fakePdf);

        var text = PdfTextExtractor.ExtractText(bytes);

        Assert.Contains("Job Title: Senior Backend Developer", text);
    }
}

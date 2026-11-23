using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace Tawzeef.Infrastructure.CvParsing;

/// <summary>A minimal, hand-rolled PDF text extractor — no third-party package. (A NuGet package
/// that would normally do this, UglyToad.PdfPig, currently resolves its own dependency tree to a
/// suspicious, non-standard version line on nuget.org — "1.7.0-custom-5" — that doesn't match its
/// real release history; rather than pull that into a real app, this reads the handful of PDF
/// primitives CvParser actually needs directly.)
///
/// Handles the common case: FlateDecode-compressed content streams showing text via the Tj/TJ
/// operators with a simple (single-byte, WinAnsi-style) font encoding — which is what most
/// text-based, non-scanned CV exports use. PDFs using embedded CID/Unicode fonts (common for some
/// Arabic text) won't decode cleanly here; that's an accepted limitation of the free/local
/// approach, not something this extractor tries to solve.</summary>
/// <summary>Public (not internal) so it can be unit tested directly — see CvParser.ParseText's
/// doc comment for the same reasoning.</summary>
public static class PdfTextExtractor
{
    private static readonly Regex StreamRegex = new(
        @"stream\r?\n(?<data>.*?)endstream", RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex TjRegex = new(
        @"\((?<text>(?:[^()\\]|\\.)*)\)\s*Tj", RegexOptions.Compiled);

    private static readonly Regex TjArrayRegex = new(
        @"\[(?<arr>(?:[^\[\]]|\\.)*)\]\s*TJ", RegexOptions.Compiled);

    private static readonly Regex TjArrayStringRegex = new(
        @"\((?<text>(?:[^()\\]|\\.)*)\)", RegexOptions.Compiled);

    /// <summary>Latin-1 treats each byte as one char, so the regex-based scan below operates on
    /// the PDF's raw bytes without corrupting them — this is a byte-oriented format, not text.</summary>
    public static string ExtractText(byte[] pdfBytes)
    {
        var raw = Encoding.Latin1.GetString(pdfBytes);
        var sb = new StringBuilder();

        foreach (Match streamMatch in StreamRegex.Matches(raw))
        {
            var streamData = streamMatch.Groups["data"].Value;
            var content = TryInflate(streamData) ?? streamData;
            AppendShowTextOperators(content, sb);
        }

        return sb.ToString();
    }

    private static string? TryInflate(string streamData)
    {
        try
        {
            var bytes = Encoding.Latin1.GetBytes(streamData);
            using var input = new MemoryStream(bytes);
            using var zlib = new ZLibStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            zlib.CopyTo(output);
            return Encoding.Latin1.GetString(output.ToArray());
        }
        catch
        {
            // Not actually FlateDecode-compressed (or truncated/malformed) — caller falls back to
            // the raw stream bytes, which is a no-op for genuinely binary content (images, fonts).
            return null;
        }
    }

    private static void AppendShowTextOperators(string content, StringBuilder sb)
    {
        foreach (Match m in TjRegex.Matches(content))
        {
            sb.Append(DecodePdfString(m.Groups["text"].Value));
            sb.Append('\n');
        }

        foreach (Match arrayMatch in TjArrayRegex.Matches(content))
        {
            foreach (Match piece in TjArrayStringRegex.Matches(arrayMatch.Groups["arr"].Value))
            {
                sb.Append(DecodePdfString(piece.Groups["text"].Value));
            }
            sb.Append('\n');
        }
    }

    /// <summary>PDF literal-string escapes: \n \r \t \b \f \( \) \\ and up to 3 octal digits.</summary>
    private static string DecodePdfString(string s)
    {
        var sb = new StringBuilder(s.Length);
        for (var i = 0; i < s.Length; i++)
        {
            if (s[i] != '\\' || i + 1 >= s.Length)
            {
                sb.Append(s[i]);
                continue;
            }

            var next = s[i + 1];
            switch (next)
            {
                case 'n': sb.Append('\n'); i++; break;
                case 'r': sb.Append('\r'); i++; break;
                case 't': sb.Append('\t'); i++; break;
                case 'b': sb.Append('\b'); i++; break;
                case 'f': sb.Append('\f'); i++; break;
                case '(': sb.Append('('); i++; break;
                case ')': sb.Append(')'); i++; break;
                case '\\': sb.Append('\\'); i++; break;
                default:
                    if (next is >= '0' and <= '7')
                    {
                        var j = i + 1;
                        var digits = 0;
                        var value = 0;
                        while (j < s.Length && digits < 3 && s[j] is >= '0' and <= '7')
                        {
                            value = value * 8 + (s[j] - '0');
                            j++;
                            digits++;
                        }
                        sb.Append((char)value);
                        i = j - 1;
                    }
                    else
                    {
                        sb.Append(next);
                        i++;
                    }
                    break;
            }
        }
        return sb.ToString();
    }
}

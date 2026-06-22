using NepaliUtility.Models;
using NepaliUtility.Services;

namespace NepaliUtility.Formatting;

/// <summary>
/// Formats a <see cref="NepaliDate"/> into a human-readable string using a token-based
/// pattern language adapted for the Bikram Sambat calendar.
/// </summary>
/// <remarks>
/// <para><b>Supported tokens</b> (longer tokens always win over shorter ones):</para>
/// <list type="table">
///   <listheader><term>Token</term><description>Example output</description></listheader>
///   <item><term>yyyy</term><description>4-digit BS year — <c>2082</c></description></item>
///   <item><term>yy</term><description>2-digit BS year — <c>82</c></description></item>
///   <item><term>MMMM</term><description>Full month name — <c>Baisakh</c> / <c>बैशाख</c></description></item>
///   <item><term>MMM</term><description>Abbreviated month — <c>Bai</c> / <c>बैशाख</c></description></item>
///   <item><term>MM</term><description>Zero-padded month number — <c>01</c></description></item>
///   <item><term>M</term><description>Month number — <c>1</c></description></item>
///   <item><term>dd</term><description>Zero-padded day — <c>05</c></description></item>
///   <item><term>d</term><description>Day number — <c>5</c></description></item>
///   <item><term>EEEE</term><description>Full weekday — <c>Tuesday</c> / <c>मंगलबार</c></description></item>
///   <item><term>EEE</term><description>Short weekday — <c>Tue</c> / <c>मंगल</c></description></item>
///   <item><term>EE</term><description>Minimal weekday — <c>Tu</c> / <c>मं</c></description></item>
/// </list>
/// <para>
/// Wrap literal text in single quotes to prevent token substitution.
/// Use <c>''</c> (two consecutive single quotes) to emit a literal apostrophe.
/// </para>
/// <para>Example: <c>"EEEE, d MMMM yyyy"</c> → <c>"Tuesday, 15 Baisakh 2082"</c></para>
/// </remarks>
public static class NepaliDateFormatter
{
    // ── Weekday name tables (Sunday = index 0) ────────────────────────────────
    private static readonly string[] _DayFull    = ["Sunday",    "Monday",   "Tuesday",  "Wednesday", "Thursday",  "Friday",   "Saturday" ];
    private static readonly string[] _DayShort   = ["Sun",       "Mon",      "Tue",       "Wed",       "Thu",       "Fri",       "Sat"      ];
    private static readonly string[] _DayMin     = ["Su",        "Mo",       "Tu",        "We",        "Th",        "Fr",        "Sa"       ];
    private static readonly string[] _DayFullNp  = ["आइतबार",   "सोमबार",   "मंगलबार",  "बुधबार",   "बिहिबार",  "शुक्रबार", "शनिबार"  ];
    private static readonly string[] _DayShortNp = ["आइत",      "सोम",      "मंगल",      "बुध",       "बिहि",      "शुक्र",    "शनि"      ];
    private static readonly string[] _DayMinNp   = ["आइ",       "सो",       "मं",        "बु",        "बि",        "शु",       "श"        ];

    // Distinct 3-char abbreviations for all 12 BS months
    private static readonly string[] _MonthAbbrevEn =
        ["Bai", "Jes", "Ash", "Shr", "Bha", "Asw", "Kar", "Mgs", "Pou", "Mag", "Fal", "Cha"];

    /// <summary>
    /// Formats <paramref name="date"/> using the given <paramref name="pattern"/>.
    /// </summary>
    /// <param name="date">The BS date to format.</param>
    /// <param name="pattern">A token-based format string.</param>
    /// <param name="nepali">
    ///   When <c>true</c>, month and weekday names are rendered in Devanagari script
    ///   and all digits are converted to Nepali (Devanagari) numerals.
    /// </param>
    public static string Format(NepaliDate date, string pattern, bool nepali = false)
    {
        ArgumentNullException.ThrowIfNull(date);
        if (string.IsNullOrEmpty(pattern)) return string.Empty;

        var ad  = BsAdConverter.BsToAd(date);
        int dow = (int)ad.DayOfWeek;   // 0 = Sunday

        var sb = new System.Text.StringBuilder(pattern.Length + 16);
        int i  = 0;

        while (i < pattern.Length)
        {
            char c = pattern[i];

            // ── Quoted literal ─────────────────────────────────────────────────
            if (c == '\'')
            {
                i++;
                if (i < pattern.Length && pattern[i] == '\'')  // '' → literal apostrophe
                {
                    sb.Append('\'');
                    i++;
                    continue;
                }
                while (i < pattern.Length && pattern[i] != '\'')
                    sb.Append(pattern[i++]);
                if (i < pattern.Length) i++;   // consume closing quote
                continue;
            }

            // ── Token dispatch (longest match takes priority) ──────────────────
            if (Peek(pattern, i, "yyyy"))
            {
                AppendDigits(sb, date.Year.ToString("D4"), nepali);
                i += 4;
            }
            else if (Peek(pattern, i, "yy"))
            {
                AppendDigits(sb, (date.Year % 100).ToString("D2"), nepali);
                i += 2;
            }
            else if (Peek(pattern, i, "MMMM"))
            {
                sb.Append(nepali ? NepaliDate.MonthNamesNepali[date.Month - 1] : NepaliDate.MonthNames[date.Month - 1]);
                i += 4;
            }
            else if (Peek(pattern, i, "MMM"))
            {
                // Nepali month names are already compact — return the full Devanagari name for MMM.
                sb.Append(nepali ? NepaliDate.MonthNamesNepali[date.Month - 1] : _MonthAbbrevEn[date.Month - 1]);
                i += 3;
            }
            else if (Peek(pattern, i, "MM"))
            {
                AppendDigits(sb, date.Month.ToString("D2"), nepali);
                i += 2;
            }
            else if (Peek(pattern, i, "M"))
            {
                AppendDigits(sb, date.Month.ToString(), nepali);
                i++;
            }
            else if (Peek(pattern, i, "dd"))
            {
                AppendDigits(sb, date.Day.ToString("D2"), nepali);
                i += 2;
            }
            else if (Peek(pattern, i, "d"))
            {
                AppendDigits(sb, date.Day.ToString(), nepali);
                i++;
            }
            else if (Peek(pattern, i, "EEEE"))
            {
                sb.Append(nepali ? _DayFullNp[dow] : _DayFull[dow]);
                i += 4;
            }
            else if (Peek(pattern, i, "EEE"))
            {
                sb.Append(nepali ? _DayShortNp[dow] : _DayShort[dow]);
                i += 3;
            }
            else if (Peek(pattern, i, "EE"))
            {
                sb.Append(nepali ? _DayMinNp[dow] : _DayMin[dow]);
                i += 2;
            }
            else
            {
                sb.Append(c);
                i++;
            }
        }

        return sb.ToString();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static bool Peek(string s, int at, string token)
        => at + token.Length <= s.Length
        && s.AsSpan(at, token.Length).Equals(token.AsSpan(), StringComparison.Ordinal);

    private static void AppendDigits(System.Text.StringBuilder sb, string digits, bool nepali)
    {
        if (!nepali) { sb.Append(digits); return; }
        foreach (char ch in digits)
            sb.Append(ch is >= '0' and <= '9' ? (char)('०' + (ch - '0')) : ch);
    }
}

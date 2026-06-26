using NepaliUtility.Models;
using NepaliUtility.Services;

namespace NepaliUtility.Formatting;

/// <summary>
/// Formats a <see cref="NepaliDate"/> or <see cref="NepaliDateTime"/> using a token-based pattern.
/// </summary>
public static class NepaliDateFormatter
{
    // Weekday tables (Sunday = 0)
    private static readonly string[] _DayFull    = ["Sunday",   "Monday",  "Tuesday", "Wednesday", "Thursday", "Friday",   "Saturday"];
    private static readonly string[] _DayShort   = ["Sun",      "Mon",     "Tue",     "Wed",       "Thu",      "Fri",      "Sat"     ];
    private static readonly string[] _DayMin     = ["Su",       "Mo",      "Tu",      "We",        "Th",       "Fr",       "Sa"      ];
    private static readonly string[] _DayFullNp  = ["आइतबार",  "सोमबार",  "मंगलबार", "बुधबार",   "बिहिबार",  "शुक्रबार", "शनिबार" ];
    private static readonly string[] _DayShortNp = ["आइत",     "सोम",     "मंगल",    "बुध",       "बिहि",     "शुक्र",    "शनि"    ];
    private static readonly string[] _DayMinNp   = ["आइ",      "सो",      "मं",      "बु",        "बि",       "शु",       "श"      ];

    private static readonly string[] _MonthAbbrevEn =
        ["Bai", "Jes", "Ash", "Shr", "Bha", "Asw", "Kar", "Mgs", "Pou", "Mag", "Fal", "Cha"];

    // Formal / Sanskrit names (MMMMM)
    private static readonly string[] _MonthFormalEn =
        ["Vaishakha", "Jyeshtha", "Ashadha", "Shravana", "Bhadrapada",
         "Ashvina", "Kartika", "Marga", "Pausha", "Magha", "Phalguna", "Chaitra"];
    private static readonly string[] _MonthFormalNp =
        ["वैशाख", "ज्येष्ठ", "आषाढ", "श्रावण", "भाद्रपद",
         "आश्विन", "कार्तिक", "मार्ग", "पौष", "माघ", "फाल्गुन", "चैत्र"];

    private static readonly string[] _QuarterOrdinal = ["1st", "2nd", "3rd", "4th"];

    /// <summary>Formats a BS date using the given pattern.</summary>
    public static string Format(NepaliDate date, string pattern, bool nepali = false)
    {
        ArgumentNullException.ThrowIfNull(date);
        if (string.IsNullOrEmpty(pattern)) return string.Empty;
        var ad  = BsAdConverter.BsToAd(date);
        int dow = (int)ad.DayOfWeek;
        return FormatCore(date.Year, date.Month, date.Day, dow, null, pattern, nepali);
    }

    /// <summary>Formats a BS date-time using the given pattern.</summary>
    public static string Format(NepaliDateTime dt, string pattern, bool nepali = false)
    {
        ArgumentNullException.ThrowIfNull(dt);
        if (string.IsNullOrEmpty(pattern)) return string.Empty;
        var ad  = BsAdConverter.BsToAd(dt.Date);
        int dow = (int)ad.DayOfWeek;
        return FormatCore(dt.Year, dt.Month, dt.Day, dow, dt.Time, pattern, nepali);
    }

    private static string FormatCore(
        int year, int month, int day, int dow,
        TimeOnly? time, string pattern, bool nepali)
    {
        int quarter = (month - 1) / 3 + 1;
        int h24   = time?.Hour ?? 0;
        int h12   = h24 % 12 == 0 ? 12 : h24 % 12;
        int min   = time?.Minute ?? 0;
        int sec   = time?.Second ?? 0;
        int ms    = time?.Millisecond ?? 0;
        bool isPm = h24 >= 12;

        var sb = new System.Text.StringBuilder(pattern.Length + 16);
        int i  = 0;

        while (i < pattern.Length)
        {
            char c = pattern[i];

            if (c == '\'')
            {
                i++;
                if (i < pattern.Length && pattern[i] == '\'') { sb.Append('\''); i++; continue; }
                while (i < pattern.Length && pattern[i] != '\'') sb.Append(pattern[i++]);
                if (i < pattern.Length) i++;
                continue;
            }

            // Era: GGG > GG > G
            if      (Peek(pattern, i, "GGG")) { sb.Append(nepali ? "बिक्रम संबत" : "Bikram Sambat"); i += 3; }
            else if (Peek(pattern, i, "GG"))  { sb.Append(nepali ? "बि.सं." : "B.S."); i += 2; }
            else if (Peek(pattern, i, "G"))   { sb.Append(nepali ? "बि सं" : "BS"); i++; }

            // Year: yyyy > yy > y
            else if (Peek(pattern, i, "yyyy")) { AppendNum(sb, year.ToString("D4"), nepali); i += 4; }
            else if (Peek(pattern, i, "yy"))   { AppendNum(sb, (year % 100).ToString("D2"), nepali); i += 2; }
            else if (Peek(pattern, i, "y"))    { AppendNum(sb, year.ToString(), nepali); i++; }

            // Quarter: QQQQ > QQQ > QQ > Q
            else if (Peek(pattern, i, "QQQQ")) { sb.Append($"{_QuarterOrdinal[quarter - 1]} quarter"); i += 4; }
            else if (Peek(pattern, i, "QQQ"))  { sb.Append($"Q{quarter}"); i += 3; }
            else if (Peek(pattern, i, "QQ"))   { AppendNum(sb, quarter.ToString("D2"), nepali); i += 2; }
            else if (Peek(pattern, i, "Q"))    { AppendNum(sb, quarter.ToString(), nepali); i++; }

            // Month (capital M): MMMMM > MMMM > MMM > MM > M
            else if (Peek(pattern, i, "MMMMM")) { sb.Append(nepali ? _MonthFormalNp[month - 1] : _MonthFormalEn[month - 1]); i += 5; }
            else if (Peek(pattern, i, "MMMM"))  { sb.Append(nepali ? NepaliDate.MonthNamesNepali[month - 1] : NepaliDate.MonthNames[month - 1]); i += 4; }
            else if (Peek(pattern, i, "MMM"))   { sb.Append(nepali ? NepaliDate.MonthNamesNepali[month - 1] : _MonthAbbrevEn[month - 1]); i += 3; }
            else if (Peek(pattern, i, "MM"))    { AppendNum(sb, month.ToString("D2"), nepali); i += 2; }
            else if (Peek(pattern, i, "M"))     { AppendNum(sb, month.ToString(), nepali); i++; }

            // Day: dd > d
            else if (Peek(pattern, i, "dd")) { AppendNum(sb, day.ToString("D2"), nepali); i += 2; }
            else if (Peek(pattern, i, "d"))  { AppendNum(sb, day.ToString(), nepali); i++; }

            // Weekday: EEEE > EEE > EE > E  (E = same as EEE short)
            else if (Peek(pattern, i, "EEEE")) { sb.Append(nepali ? _DayFullNp[dow]  : _DayFull[dow]);  i += 4; }
            else if (Peek(pattern, i, "EEE"))  { sb.Append(nepali ? _DayShortNp[dow] : _DayShort[dow]); i += 3; }
            else if (Peek(pattern, i, "EE"))   { sb.Append(nepali ? _DayMinNp[dow]   : _DayMin[dow]);   i += 2; }
            else if (Peek(pattern, i, "E"))    { sb.Append(nepali ? _DayShortNp[dow] : _DayShort[dow]); i++; }

            // 24-hour: HH > H
            else if (Peek(pattern, i, "HH")) { AppendNum(sb, h24.ToString("D2"), nepali); i += 2; }
            else if (Peek(pattern, i, "H"))  { AppendNum(sb, h24.ToString(), nepali); i++; }

            // 12-hour: hh > h
            else if (Peek(pattern, i, "hh")) { AppendNum(sb, h12.ToString("D2"), nepali); i += 2; }
            else if (Peek(pattern, i, "h"))  { AppendNum(sb, h12.ToString(), nepali); i++; }

            // AM/PM: aa > a
            else if (Peek(pattern, i, "aa")) { sb.Append(nepali ? (isPm ? "बेलुकी" : "बिहान") : (isPm ? "PM" : "AM")); i += 2; }
            else if (Peek(pattern, i, "a"))  { sb.Append(nepali ? (isPm ? "बेलुकी" : "बिहान") : (isPm ? "pm" : "am")); i++; }

            // Minute (lowercase): mm > m
            else if (Peek(pattern, i, "mm")) { AppendNum(sb, min.ToString("D2"), nepali); i += 2; }
            else if (Peek(pattern, i, "m"))  { AppendNum(sb, min.ToString(), nepali); i++; }

            // Second: ss > s; Fractional: S
            else if (Peek(pattern, i, "ss")) { AppendNum(sb, sec.ToString("D2"), nepali); i += 2; }
            else if (Peek(pattern, i, "s"))  { AppendNum(sb, sec.ToString(), nepali); i++; }
            else if (Peek(pattern, i, "S"))  { AppendNum(sb, ms.ToString(), nepali); i++; }

            else { sb.Append(c); i++; }
        }

        return sb.ToString();
    }

    private static bool Peek(string s, int at, string token)
        => at + token.Length <= s.Length
        && s.AsSpan(at, token.Length).Equals(token.AsSpan(), StringComparison.Ordinal);

    private static void AppendNum(System.Text.StringBuilder sb, string digits, bool nepali)
    {
        if (!nepali) { sb.Append(digits); return; }
        foreach (char ch in digits)
            sb.Append(ch is >= '0' and <= '9' ? (char)('०' + (ch - '0')) : ch);
    }
}

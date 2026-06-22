namespace NepaliUtility.Models;

/// <summary>
/// Represents a date in the Bikram Sambat (BS) calendar system.
/// </summary>
public record NepaliDate(int Year, int Month, int Day) : IComparable<NepaliDate>
{
    public static readonly string[] MonthNames =
    [
        "Baisakh", "Jestha", "Ashadh", "Shrawan",
        "Bhadra",  "Ashwin", "Kartik", "Mangsir",
        "Poush",   "Magh",   "Falgun", "Chaitra"
    ];

    public static readonly string[] MonthNamesNepali =
    [
        "बैशाख", "जेठ", "असार", "श्रावण",
        "भाद्र", "आश्विन", "कार्तिक", "मंसिर",
        "पुष",   "माघ",   "फाल्गुन", "चैत्र"
    ];

    /// <summary>AD (Gregorian) month names in Nepali (Devanagari) script.</summary>
    public static readonly string[] AdMonthNamesNepali =
    [
        "जनवरी",  "फेब्रुअरी", "मार्च",     "अप्रिल",
        "मे",      "जुन",      "जुलाई",     "अगस्ट",
        "सेप्टेम्बर", "अक्टोबर",  "नोभेम्बर",  "डिसेम्बर"
    ];

    public string MonthName       => MonthNames[Month - 1];
    public string MonthNameNepali => MonthNamesNepali[Month - 1];

    public override string ToString() => $"{Year}/{Month:D2}/{Day:D2}";

    public string ToDisplayString() => $"{Day} {MonthName} {Year}";

    public bool IsValid()
    {
        if (Month < 1 || Month > 12) return false;
        if (Day < 1) return false;
        try
        {
            int maxDay = Data.BsCalendarData.GetDaysInMonth(Year, Month);
            return Day <= maxDay;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Returns this date with the month shifted by <paramref name="months"/>, clamping the day if needed.</summary>
    public NepaliDate AddMonths(int months)
    {
        int y = Year, m = Month + months, d = Day;
        while (m > 12) { m -= 12; y++; }
        while (m < 1)  { m += 12; y--; }
        d = Data.BsCalendarData.ClampDay(y, m, d);
        return new NepaliDate(y, m, d);
    }

    /// <summary>Returns this date in a different BS year, clamping the day if the month is shorter.</summary>
    public NepaliDate AddYears(int years)
    {
        int y = Year + years;
        int d = Data.BsCalendarData.ClampDay(y, Month, Day);
        return new NepaliDate(y, Month, d);
    }

    /// <summary>
    /// Parses a BS date string in <c>YYYY/MM/DD</c> or <c>YYYY-MM-DD</c> format.
    /// Throws <see cref="FormatException"/> if the string is not a valid BS date.
    /// </summary>
    public static NepaliDate Parse(string s)
    {
        if (!TryParse(s, out var result))
            throw new FormatException($"'{s}' is not a valid BS date. Expected YYYY/MM/DD or YYYY-MM-DD.");
        return result!;
    }

    /// <summary>
    /// Tries to parse a BS date string in <c>YYYY/MM/DD</c> or <c>YYYY-MM-DD</c> format.
    /// </summary>
    public static bool TryParse(string s, out NepaliDate? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(s)) return false;
        var parts = s.Split('/', '-');
        if (parts.Length != 3) return false;
        if (!int.TryParse(parts[0], out int y) ||
            !int.TryParse(parts[1], out int m) ||
            !int.TryParse(parts[2], out int d)) return false;
        var candidate = new NepaliDate(y, m, d);
        if (!candidate.IsValid()) return false;
        result = candidate;
        return true;
    }

    /// <summary>Compares this date to another BS date chronologically.</summary>
    public int CompareTo(NepaliDate? other)
    {
        if (other is null) return 1;
        int cmp = Year.CompareTo(other.Year);
        if (cmp != 0) return cmp;
        cmp = Month.CompareTo(other.Month);
        return cmp != 0 ? cmp : Day.CompareTo(other.Day);
    }

    public static bool operator <(NepaliDate a, NepaliDate b)  => a.CompareTo(b) < 0;
    public static bool operator >(NepaliDate a, NepaliDate b)  => a.CompareTo(b) > 0;
    public static bool operator <=(NepaliDate a, NepaliDate b) => a.CompareTo(b) <= 0;
    public static bool operator >=(NepaliDate a, NepaliDate b) => a.CompareTo(b) >= 0;
}

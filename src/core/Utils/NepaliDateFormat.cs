using NepaliUtility.Models;

namespace NepaliUtility.Formatting;

/// <summary>
/// Provides predefined and custom date/time formats for <see cref="NepaliDateTime"/> and <see cref="NepaliDate"/>.
/// </summary>
/// <example>
/// <code>
/// var fmt = NepaliDateFormat.MEd();
/// fmt.Format(dt);          // "Sat, 4/18"
///
/// var fmt2 = new NepaliDateFormat("yyyy.MM.dd GGG 'at' HH:mm:ss");
/// fmt2.Format(dt);         // "2072.01.12 Bikram Sambat at 11:56:25"
/// </code>
/// </example>
public sealed class NepaliDateFormat
{
    private readonly string _pattern;
    private readonly bool   _nepali;

    public NepaliDateFormat(string pattern, bool nepali = false)
    {
        _pattern = pattern;
        _nepali  = nepali;
    }

    public string Format(NepaliDateTime dateTime) => NepaliDateFormatter.Format(dateTime, _pattern, _nepali);
    public string Format(NepaliDate date)          => NepaliDateFormatter.Format(date, _pattern, _nepali);

    // ── Predefined date formats ───────────────────────────────────────────────

    /// <summary>Day of month: 18</summary>
    public static NepaliDateFormat d()           => new("d");
    /// <summary>Short weekday: Sat</summary>
    public static NepaliDateFormat E()           => new("E");
    /// <summary>Full weekday: Saturday</summary>
    public static NepaliDateFormat EEEE()        => new("EEEE");
    /// <summary>Abbreviated month: Shr</summary>
    public static NepaliDateFormat LLL()         => new("MMM");
    /// <summary>Full month: Shrawan</summary>
    public static NepaliDateFormat LLLL()        => new("MMMM");
    /// <summary>Month number: 4</summary>
    public static NepaliDateFormat M()           => new("M");
    /// <summary>Month/day: 4/18</summary>
    public static NepaliDateFormat Md()          => new("M/d");
    /// <summary>Weekday, month/day: Sat, 4/18</summary>
    public static NepaliDateFormat MEd()         => new("E, M/d");
    /// <summary>Abbreviated month: Shr</summary>
    public static NepaliDateFormat MMM()         => new("MMM");
    /// <summary>Abbreviated month + day: Shr 18</summary>
    public static NepaliDateFormat MMMd()        => new("MMM d");
    /// <summary>Weekday, abbreviated month + day: Sat, Shr 18</summary>
    public static NepaliDateFormat MMMEd()       => new("E, MMM d");
    /// <summary>Full month: Shrawan</summary>
    public static NepaliDateFormat MMMM()        => new("MMMM");
    /// <summary>Full month + day: Shrawan 18</summary>
    public static NepaliDateFormat MMMMd()       => new("MMMM d");
    /// <summary>Full weekday, full month + day: Saturday, Shrawan 18</summary>
    public static NepaliDateFormat MMMMEEEEd()   => new("EEEE, MMMM d");
    /// <summary>Quarter abbreviation: Q2</summary>
    public static NepaliDateFormat QQQ()         => new("QQQ");
    /// <summary>Quarter ordinal: 2nd quarter</summary>
    public static NepaliDateFormat QQQQ()        => new("QQQQ");
    /// <summary>Year: 2076</summary>
    public static NepaliDateFormat y()           => new("yyyy");
    /// <summary>Year/month: 2076/04</summary>
    public static NepaliDateFormat yM()          => new("yyyy/MM");
    /// <summary>Year/month/day: 2076/04/18</summary>
    public static NepaliDateFormat yMd()         => new("yyyy/MM/dd");
    /// <summary>Weekday, year/month/day: Sat, 2076/04/18</summary>
    public static NepaliDateFormat yMEd()        => new("E, yyyy/MM/dd");
    /// <summary>Abbreviated month + year: Shr 2076</summary>
    public static NepaliDateFormat yMMM()        => new("MMM yyyy");
    /// <summary>Abbreviated month + day + year: Shr 18, 2076</summary>
    public static NepaliDateFormat yMMMd()       => new("MMM d, yyyy");
    /// <summary>Weekday, abbreviated month + day + year: Sat, Shr 18, 2076</summary>
    public static NepaliDateFormat yMMMEd()      => new("E, MMM d, yyyy");
    /// <summary>Full month + year: Shrawan 2076</summary>
    public static NepaliDateFormat yMMMMM()      => new("MMMM yyyy");
    /// <summary>Full month + day + year: Shrawan 18, 2076</summary>
    public static NepaliDateFormat yMMMMd()      => new("MMMM d, yyyy");
    /// <summary>Full weekday, full month + day + year: Saturday, Shrawan 18, 2076</summary>
    public static NepaliDateFormat yMMMMEEEEd()  => new("EEEE, MMMM d, yyyy");
    /// <summary>Quarter + year: Q2 2076</summary>
    public static NepaliDateFormat yQQQ()        => new("QQQ yyyy");
    /// <summary>Quarter ordinal + year: 2nd quarter 2076</summary>
    public static NepaliDateFormat yQQQQ()       => new("QQQQ yyyy");

    // ── Predefined time formats ───────────────────────────────────────────────

    /// <summary>24-hour: 21</summary>
    public static NepaliDateFormat H()   => new("H");
    /// <summary>24-hour + minute: 21:04</summary>
    public static NepaliDateFormat Hm()  => new("H:mm");
    /// <summary>24-hour + minute + second: 21:17:56</summary>
    public static NepaliDateFormat Hms() => new("H:mm:ss");
    /// <summary>12-hour + AM/PM: 9 PM</summary>
    public static NepaliDateFormat j()   => new("h aa");
    /// <summary>12-hour + minute + AM/PM: 9:17 PM</summary>
    public static NepaliDateFormat jm()  => new("h:mm aa");
    /// <summary>12-hour + minute + second + AM/PM: 9:17:56 PM</summary>
    public static NepaliDateFormat jms() => new("h:mm:ss aa");
    /// <summary>Minute: 9</summary>
    public static NepaliDateFormat m()   => new("m");
    /// <summary>Minute + second: 9:17</summary>
    public static NepaliDateFormat ms()  => new("m:ss");
    /// <summary>Second: 56</summary>
    public static NepaliDateFormat s()   => new("s");
}

namespace NepaliUtility.Models;

/// <summary>
/// Represents a Bikram Sambat date combined with a time-of-day component.
/// Use <see cref="Services.BsAdConverter.Now"/> to obtain the current BS date and time.
/// </summary>
public record NepaliDateTime(NepaliDate Date, TimeOnly Time)
{
    public int Year        => Date.Year;
    public int Month       => Date.Month;
    public int Day         => Date.Day;
    public int Hour        => Time.Hour;
    public int Minute      => Time.Minute;
    public int Second      => Time.Second;
    public int Millisecond => Time.Millisecond;

    public override string ToString() =>
        $"{Date} {Hour:D2}:{Minute:D2}:{Second:D2}";

    /// <summary>
    /// Parses a string in <c>YYYY/MM/DD HH:mm:ss</c> or <c>YYYY/MM/DD HH:mm</c> format.
    /// Throws <see cref="FormatException"/> if the string is invalid.
    /// </summary>
    public static NepaliDateTime Parse(string s)
    {
        if (!TryParse(s, out var result))
            throw new FormatException($"'{s}' is not a valid NepaliDateTime. Expected 'YYYY/MM/DD HH:mm:ss'.");
        return result!;
    }

    /// <summary>
    /// Tries to parse a string in <c>YYYY/MM/DD HH:mm:ss</c> or <c>YYYY/MM/DD HH:mm</c> format.
    /// </summary>
    public static bool TryParse(string s, out NepaliDateTime? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(s)) return false;

        var parts = s.Trim().Split(' ');
        if (!NepaliDate.TryParse(parts[0], out var date) || date is null) return false;

        var time = TimeOnly.MinValue;
        if (parts.Length >= 2 && !TimeOnly.TryParse(parts[1], out time)) return false;

        result = new NepaliDateTime(date, time);
        return true;
    }
}

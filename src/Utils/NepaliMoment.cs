using NepaliDatePicker.Models;
using NepaliDatePicker.Services;

namespace NepaliDatePicker.Formatting;

/// <summary>
/// Produces human-readable relative-time strings ("3 days ago", "in 2 months", "Yesterday")
/// from <see cref="NepaliDate"/> or <see cref="DateTime"/> values.
/// </summary>
/// <remarks>
/// <para>
/// Thresholds follow natural speech conventions:
/// under a minute → "Just now"; under an hour → minutes;
/// under a day → hours; exactly 1 day → "Yesterday/Tomorrow";
/// under 7 days → days; under 30 days → weeks;
/// under 365 days → months; otherwise → years.
/// </para>
/// </remarks>
public static class NepaliMoment
{
    /// <summary>
    /// Returns how long ago (or until) <paramref name="date"/> is,
    /// relative to <paramref name="reference"/> (defaults to today in BS).
    /// </summary>
    /// <param name="date">The BS date to describe.</param>
    /// <param name="reference">Comparison point; defaults to today if <c>null</c>.</param>
    /// <param name="nepali">When <c>true</c>, returns Devanagari text and numerals.</param>
    public static string Elapsed(NepaliDate date, NepaliDate? reference = null, bool nepali = false)
    {
        var refDate = reference ?? BsAdConverter.AdToBs(DateTime.Today);
        return ElapsedCore(
            BsAdConverter.BsToAd(date).Date,
            BsAdConverter.BsToAd(refDate).Date,
            nepali);
    }

    /// <summary>
    /// Returns how long ago (or until) <paramref name="date"/> is,
    /// relative to <paramref name="reference"/> (defaults to <see cref="DateTime.Today"/>).
    /// </summary>
    public static string Elapsed(DateTime date, DateTime? reference = null, bool nepali = false)
        => ElapsedCore(date.Date, (reference ?? DateTime.Today).Date, nepali);

    // ─────────────────────────────────────────────────────────────────────────

    private static string ElapsedCore(DateTime date, DateTime reference, bool nepali)
    {
        var    span    = reference - date;
        bool   future  = span.TotalSeconds < 0;
        double absSec  = Math.Abs(span.TotalSeconds);
        int    absMin  = (int)Math.Abs(span.TotalMinutes);
        int    absHr   = (int)Math.Abs(span.TotalHours);
        int    absDays = (int)Math.Abs(span.TotalDays);

        if (absSec < 60)
            return nepali ? "भर्खरै" : "Just now";

        if (absMin < 60)
            return Unit(absMin, "minute", "मिनेट", future, nepali);

        if (absHr < 24)
            return Unit(absHr, "hour", "घण्टा", future, nepali);

        if (absDays == 1)
            return future
                ? (nepali ? "भोलि" : "Tomorrow")
                : (nepali ? "हिजो" : "Yesterday");

        if (absDays < 7)
            return Unit(absDays, "day", "दिन", future, nepali);

        if (absDays < 30)
            return Unit(absDays / 7, "week", "हप्ता", future, nepali);

        if (absDays < 365)
            return Unit(absDays / 30, "month", "महिना", future, nepali);

        return Unit(absDays / 365, "year", "वर्ष", future, nepali);
    }

    private static string Unit(int n, string enWord, string npWord, bool future, bool nepali)
    {
        if (nepali)
        {
            string np = ToDevanagari(n.ToString());
            return future ? $"{np} {npWord}मा" : $"{np} {npWord} पहिले";
        }
        string plural = n == 1 ? "" : "s";
        return future ? $"In {n} {enWord}{plural}" : $"{n} {enWord}{plural} ago";
    }

    private static string ToDevanagari(string s)
    {
        var sb = new System.Text.StringBuilder(s.Length);
        foreach (char ch in s)
            sb.Append(ch is >= '0' and <= '9' ? (char)('०' + (ch - '0')) : ch);
        return sb.ToString();
    }
}

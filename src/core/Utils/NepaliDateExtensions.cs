using NepaliUtility.Models;
using NepaliUtility.Services;

namespace NepaliUtility.Formatting;

/// <summary>
/// Extension methods that enrich <see cref="NepaliDate"/> with formatting,
/// relative-time, and date-arithmetic utilities.
/// Add <c>using NepaliUtility.Formatting;</c> to bring these into scope.
/// </summary>
public static class NepaliDateExtensions
{
    /// <summary>
    /// Formats this date using the given pattern string.
    /// Delegates to <see cref="NepaliDateFormatter.Format"/>.
    /// </summary>
    public static string Format(this NepaliDate date, string pattern, bool nepali = false)
        => NepaliDateFormatter.Format(date, pattern, nepali);

    /// <summary>
    /// Returns a relative-time string such as <c>"3 days ago"</c> or <c>"in 2 months"</c>.
    /// Delegates to <see cref="NepaliMoment.Elapsed(NepaliDate, NepaliDate?, bool)"/>.
    /// </summary>
    public static string Elapsed(this NepaliDate date, NepaliDate? reference = null, bool nepali = false)
        => NepaliMoment.Elapsed(date, reference, nepali);

    /// <summary>Returns this BS date shifted forward or backward by <paramref name="days"/> days.</summary>
    public static NepaliDate AddDays(this NepaliDate date, int days)
        => BsAdConverter.AdToBs(BsAdConverter.BsToAd(date).AddDays(days));

    /// <summary>Returns the day of the week for this BS date.</summary>
    public static DayOfWeek GetDayOfWeek(this NepaliDate date)
        => BsAdConverter.BsToAd(date).DayOfWeek;

    /// <summary>
    /// Returns the number of days from this date to <paramref name="other"/>
    /// (positive if <paramref name="other"/> is in the future).
    /// </summary>
    public static int DaysTo(this NepaliDate date, NepaliDate other)
        => (int)(BsAdConverter.BsToAd(other) - BsAdConverter.BsToAd(date)).TotalDays;
}

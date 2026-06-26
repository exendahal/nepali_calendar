using NepaliUtility.Models;
using NepaliUtility.Services;

namespace NepaliUtility.Formatting;

/// <summary>
/// Extension methods on <see cref="NepaliDate"/> and <see cref="NepaliDateTime"/>.
/// Add <c>using NepaliUtility.Formatting;</c> to bring these into scope.
/// </summary>
public static class NepaliDateExtensions
{
    // ── NepaliDate extensions ─────────────────────────────────────────────────

    public static string Format(this NepaliDate date, string pattern, bool nepali = false)
        => NepaliDateFormatter.Format(date, pattern, nepali);

    public static string Elapsed(this NepaliDate date, NepaliDate? reference = null, bool nepali = false)
        => NepaliMoment.Elapsed(date, reference, nepali);

    public static NepaliDate AddDays(this NepaliDate date, int days)
        => BsAdConverter.AdToBs(BsAdConverter.BsToAd(date).AddDays(days));

    public static DayOfWeek GetDayOfWeek(this NepaliDate date)
        => BsAdConverter.BsToAd(date).DayOfWeek;

    public static int DaysTo(this NepaliDate date, NepaliDate other)
        => (int)(BsAdConverter.BsToAd(other) - BsAdConverter.BsToAd(date)).TotalDays;

    // ── NepaliDateTime extensions ─────────────────────────────────────────────

    public static string Format(this NepaliDateTime dt, string pattern, bool nepali = false)
        => NepaliDateFormatter.Format(dt, pattern, nepali);

    public static string Elapsed(this NepaliDateTime dt, NepaliDate? reference = null, bool nepali = false)
        => NepaliMoment.Elapsed(dt.Date, reference, nepali);

    public static DayOfWeek GetDayOfWeek(this NepaliDateTime dt)
        => BsAdConverter.BsToAd(dt.Date).DayOfWeek;
}

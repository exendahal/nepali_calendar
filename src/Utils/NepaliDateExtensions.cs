namespace NepaliDatePicker.Formatting;

/// <summary>
/// Extension methods that enrich <see cref="NepaliDatePicker.Models.NepaliDate"/>
/// with formatting and relative-time utilities.
/// Add <c>using NepaliDatePicker.Formatting;</c> to bring these into scope.
/// </summary>
public static class NepaliDateExtensions
{
    /// <summary>
    /// Formats this date using the given pattern string.
    /// Delegates to <see cref="NepaliDateFormatter.Format"/>.
    /// </summary>
    /// <param name="date">The BS date to format.</param>
    /// <param name="pattern">
    ///   Token-based format string — e.g. <c>"d MMMM yyyy"</c>,
    ///   <c>"EEEE, dd/MM/yyyy"</c>. See <see cref="NepaliDateFormatter"/> for all tokens.
    /// </param>
    /// <param name="nepali">
    ///   When <c>true</c>, names are Devanagari and digits are Nepali numerals.
    /// </param>
    public static string Format(
        this NepaliDatePicker.Models.NepaliDate date,
        string pattern,
        bool   nepali = false)
        => NepaliDateFormatter.Format(date, pattern, nepali);

    /// <summary>
    /// Returns a relative-time string such as <c>"3 days ago"</c> or <c>"in 2 months"</c>.
    /// Delegates to <see cref="NepaliMoment.Elapsed(NepaliDatePicker.Models.NepaliDate, NepaliDatePicker.Models.NepaliDate?, bool)"/>.
    /// </summary>
    /// <param name="date">The BS date to describe.</param>
    /// <param name="reference">Comparison point; defaults to today if <c>null</c>.</param>
    /// <param name="nepali">When <c>true</c>, returns Devanagari text and numerals.</param>
    public static string Elapsed(
        this NepaliDatePicker.Models.NepaliDate date,
        NepaliDatePicker.Models.NepaliDate?     reference = null,
        bool                                    nepali    = false)
        => NepaliMoment.Elapsed(date, reference, nepali);
}

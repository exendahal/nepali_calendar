namespace NepaliUtility.Services;

/// <summary>
/// Helpers for Nepal Standard Time (NST) — UTC+5:45.
/// </summary>
public static class NepaliTimeZone
{
    /// <summary>Nepal Standard Time offset: UTC+5:45.</summary>
    public static readonly TimeSpan Offset = TimeSpan.FromMinutes(5 * 60 + 45);

    /// <summary>Returns the current date and time in Nepal Standard Time.</summary>
    public static DateTimeOffset Now => DateTimeOffset.UtcNow.ToOffset(Offset);

    /// <summary>Converts a UTC <see cref="DateTime"/> to Nepal Standard Time.</summary>
    public static DateTimeOffset ToNepalTime(DateTime utcDateTime) =>
        new DateTimeOffset(DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc)).ToOffset(Offset);

    /// <summary>Converts a local <see cref="DateTimeOffset"/> to Nepal Standard Time.</summary>
    public static DateTimeOffset ToNepalTime(DateTimeOffset dt) => dt.ToOffset(Offset);
}

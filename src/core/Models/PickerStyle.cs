namespace NepaliUtility.Models;

/// <summary>Visual style of the date picker body.</summary>
public enum PickerStyle
{
    /// <summary>Material Design 3 month calendar grid. Default.</summary>
    Calendar,

    /// <summary>iOS-style drum-roll wheels (year / month / day).</summary>
    Wheel,

    /// <summary>Desktop-friendly text-input mode. User types a date in YYYY-MM-DD format; OK is enabled only when the value is valid.</summary>
    Input,
}

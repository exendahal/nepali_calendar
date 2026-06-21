namespace NepaliDatePicker.Models;

/// <summary>Controls which calendar system(s) the picker and button expose.</summary>
public enum DateDisplayMode
{
    /// <summary>Only Bikram Sambat (BS/Nepali) calendar. No mode-switch toggle shown.</summary>
    BsOnly,

    /// <summary>Only Gregorian (AD) calendar. No mode-switch toggle shown.</summary>
    AdOnly,

    /// <summary>Both BS and AD available — chip toggle lets the user switch. Default.</summary>
    Both,
}

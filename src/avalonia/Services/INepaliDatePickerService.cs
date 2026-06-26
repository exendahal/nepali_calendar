using NepaliDatePicker.Models;
using NepaliUtility.Models;

namespace NepaliDatePicker.Services;

/// <summary>
/// Programmatic API for showing the Nepali date picker dialog on Avalonia desktop.
/// </summary>
public interface INepaliDatePickerService
{
    /// <summary>
    /// Opens the picker dialog and awaits the user's choice.
    /// Returns the selected BS date, or <c>null</c> if cancelled.
    /// </summary>
    Task<NepaliDate?> ShowAsync(NepaliDate? initialDate = null, NepaliDatePickerOptions? options = null);

    /// <summary>Returns the current BS date equivalent of today's AD date.</summary>
    NepaliDate Today { get; }
}

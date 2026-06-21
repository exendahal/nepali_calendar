using NepaliDatePicker.Models;

namespace NepaliDatePicker.Services;

/// <summary>
/// Programmatic API for showing the Nepali date picker bottom sheet.
/// Register via <c>builder.AddNepaliDatePicker()</c>, then inject.
/// </summary>
public interface INepaliDatePickerService
{
    /// <summary>
    /// Opens the picker bottom sheet and awaits the user's choice.
    /// Returns the selected BS date, or <c>null</c> if cancelled.
    /// </summary>
    /// <param name="initialDate">Pre-selected date, or <c>null</c> for today.</param>
    /// <param name="options">Visual and behavioural customisation; falls back to MD3 defaults.</param>
    Task<NepaliDate?> ShowAsync(NepaliDate? initialDate = null, NepaliDatePickerOptions? options = null);

    /// <summary>Returns the current BS date equivalent of today's AD date.</summary>
    NepaliDate Today { get; }
}

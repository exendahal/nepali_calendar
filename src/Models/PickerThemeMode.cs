namespace NepaliDatePicker.Models;

/// <summary>
/// Controls the visual language the date-picker uses when
/// <see cref="NepaliDatePickerOptions.NativeTheme"/> is <c>true</c>.
/// </summary>
public enum PickerThemeMode
{
    /// <summary>Material Design 3 — purple accent, colored header band. Android default.</summary>
    Material,

    /// <summary>Apple Human Interface Guidelines — blue accent, no header, wheel picker. iOS/macOS default.</summary>
    Cupertino,

    /// <summary>Windows Fluent Design — system accent color, no header, dialog presentation. Windows default.</summary>
    Fluent,
}

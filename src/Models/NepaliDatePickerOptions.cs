namespace NepaliDatePicker.Models;

/// <summary>
/// Optional customization for the Nepali Date Picker bottom sheet and button.
/// All color properties are nullable — unset values fall back to Material Design 3 defaults.
/// </summary>
public class NepaliDatePickerOptions
{
    // ── Picker palette ────────────────────────────────────────────────────────

    /// <summary>Primary accent color — selected day fill, active chip, action button text, today outline.
    /// Applied to both light and dark themes unless <see cref="PrimaryColorDark"/> is also set.</summary>
    public Color? PrimaryColor { get; set; }

    /// <summary>Primary color override specifically for the dark theme.</summary>
    public Color? PrimaryColorDark { get; set; }

    /// <summary>Text/icon color drawn on top of the primary-filled circle or chip.</summary>
    public Color? OnPrimaryColor { get; set; }

    /// <summary>Header band background for the light theme.</summary>
    public Color? HeaderBackgroundColor { get; set; }

    /// <summary>Header band background for the dark theme. Falls back to <see cref="HeaderBackgroundColor"/>.</summary>
    public Color? HeaderBackgroundColorDark { get; set; }

    /// <summary>All text and sub-text inside the header band. Applied to both themes.</summary>
    public Color? HeaderTextColor { get; set; }

    /// <summary>Picker body background for the light theme.</summary>
    public Color? SurfaceColor { get; set; }

    /// <summary>Picker body background for the dark theme. Falls back to <see cref="SurfaceColor"/>.</summary>
    public Color? SurfaceColorDark { get; set; }

    /// <summary>Main body text color for the light theme (day numbers, month/year label).</summary>
    public Color? OnSurfaceColor { get; set; }

    /// <summary>Main body text color for the dark theme. Falls back to <see cref="OnSurfaceColor"/>.</summary>
    public Color? OnSurfaceColorDark { get; set; }

    /// <summary>Secondary body text color (day-of-week initials) for the light theme.</summary>
    public Color? OnSurfaceVariantColor { get; set; }

    /// <summary>Secondary body text color for the dark theme. Falls back to <see cref="OnSurfaceVariantColor"/>.</summary>
    public Color? OnSurfaceVariantColorDark { get; set; }

    // ── Typography ────────────────────────────────────────────────────────────

    /// <summary>Custom font family applied to all labels inside the picker.</summary>
    public string? FontFamily { get; set; }

    // ── Structure ─────────────────────────────────────────────────────────────

    /// <summary>Corner radius for the top-left and top-right corners of the bottom sheet. Default: 28.</summary>
    public double SheetCornerRadius { get; set; } = 28;

    // ── Behaviour ─────────────────────────────────────────────────────────────

    /// <summary>Which calendar system(s) are available in the picker. Default: <see cref="DateDisplayMode.Both"/>.</summary>
    public DateDisplayMode DisplayMode { get; set; } = DateDisplayMode.Both;

    /// <summary>
    /// How the picker is shown on screen.
    /// <see cref="PickerPresentation.BottomSheet"/> slides up from the bottom (default).
    /// <see cref="PickerPresentation.Dialog"/> floats centered over the page.
    /// </summary>
    public PickerPresentation Presentation { get; set; } = PickerPresentation.BottomSheet;

    /// <summary>
    /// When <c>true</c>, all BS month names and numbers inside the picker are rendered
    /// in Devanagari script (e.g. बैशाख, २०८२, ०१). AD dates always stay in English.
    /// Default: <c>false</c>.
    /// </summary>
    public bool UseNepaliScript { get; set; } = false;
}

using Avalonia.Media;
using NepaliDatePicker.Models;

namespace NepaliDatePicker.Models;

/// <summary>
/// Optional customization for the Avalonia Nepali Date Picker dialog.
/// All color properties are nullable — unset values fall back to Material Design 3 defaults.
/// </summary>
public class NepaliDatePickerOptions
{
    // ── Picker palette ────────────────────────────────────────────────────────

    public Color? PrimaryColor { get; set; }
    public Color? PrimaryColorDark { get; set; }
    public Color? OnPrimaryColor { get; set; }
    public Color? HeaderBackgroundColor { get; set; }
    public Color? HeaderBackgroundColorDark { get; set; }
    public Color? HeaderTextColor { get; set; }
    public Color? SurfaceColor { get; set; }
    public Color? SurfaceColorDark { get; set; }
    public Color? OnSurfaceColor { get; set; }
    public Color? OnSurfaceColorDark { get; set; }
    public Color? OnSurfaceVariantColor { get; set; }
    public Color? OnSurfaceVariantColorDark { get; set; }

    // ── Typography ────────────────────────────────────────────────────────────

    public string? FontFamily { get; set; }

    // ── Structure ─────────────────────────────────────────────────────────────

    public double SheetCornerRadius { get; set; } = 12;

    public bool ShowHeader { get; set; } = true;

    // ── Labels ────────────────────────────────────────────────────────────────

    public string CancelLabel { get; set; } = "CANCEL";
    public string ConfirmLabel { get; set; } = "OK";

    // ── Behaviour ─────────────────────────────────────────────────────────────

    public DateDisplayMode DisplayMode { get; set; } = DateDisplayMode.Both;

    /// <summary>
    /// Visual style of the picker body.
    /// <see cref="PickerStyle.Wheel"/> falls back to <see cref="PickerStyle.Calendar"/> on Avalonia (desktop).
    /// </summary>
    public PickerStyle PickerStyle { get; set; } = PickerStyle.Calendar;

    public bool UseNepaliScript { get; set; } = false;
}

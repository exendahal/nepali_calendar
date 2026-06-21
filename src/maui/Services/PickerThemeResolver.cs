using NepaliDatePicker.Models;

namespace NepaliDatePicker.Services;

/// <summary>
/// Applies platform-native visual defaults to <see cref="NepaliDatePickerOptions"/>
/// when <see cref="NepaliDatePickerOptions.NativeTheme"/> is <c>true</c>.
/// Only nullable color fields that were not explicitly set by the caller are filled in.
/// Structural fields (corner radius, presentation, style) are always overwritten by the theme.
/// </summary>
internal static class PickerThemeResolver
{
    internal static void Apply(NepaliDatePickerOptions options)
    {
        if (!options.NativeTheme) return;

        var theme = DetectTheme();
        switch (theme)
        {
            case PickerThemeMode.Cupertino:
                ApplyCupertino(options);
                break;
            case PickerThemeMode.Fluent:
                ApplyFluent(options);
                break;
            // Material: MD3 is already the hard-coded default — no changes needed.
        }
    }

    // ── Platform detection ────────────────────────────────────────────────────

    private static PickerThemeMode DetectTheme()
    {
        if (DeviceInfo.Platform == DevicePlatform.iOS ||
            DeviceInfo.Platform == DevicePlatform.MacCatalyst)
            return PickerThemeMode.Cupertino;

        if (DeviceInfo.Platform == DevicePlatform.WinUI)
            return PickerThemeMode.Fluent;

        return PickerThemeMode.Material;
    }

    // ── Cupertino (iOS / macOS) ───────────────────────────────────────────────

    private static void ApplyCupertino(NepaliDatePickerOptions opts)
    {
        // iOS system blue — adapts light/dark automatically via separate tokens.
        opts.PrimaryColor             ??= Color.FromArgb("#007AFF");
        opts.PrimaryColorDark         ??= Color.FromArgb("#0A84FF");
        opts.OnPrimaryColor           ??= Colors.White;

        // iOS sheet surface colors.
        opts.SurfaceColor             ??= Color.FromArgb("#F2F2F7");
        opts.SurfaceColorDark         ??= Color.FromArgb("#1C1C1E");
        opts.OnSurfaceColor           ??= Color.FromArgb("#1C1C1E");
        opts.OnSurfaceColorDark       ??= Color.FromArgb("#FFFFFF");
        opts.OnSurfaceVariantColor    ??= Color.FromArgb("#3A3A3C");
        opts.OnSurfaceVariantColorDark ??= Color.FromArgb("#8E8E93");

        // iOS HIG: no colored header band; wheel selector; title-case button labels.
        opts.ShowHeader        = false;
        opts.SheetCornerRadius = 13;
        opts.CancelLabel       = "Cancel";
        opts.ConfirmLabel      = "Done";
        opts.Presentation      = PickerPresentation.BottomSheet;
        opts.PickerStyle       = PickerStyle.Wheel;
    }

    // ── Fluent (Windows) ─────────────────────────────────────────────────────

    private static void ApplyFluent(NepaliDatePickerOptions opts)
    {
        var accent = GetWindowsAccentColor();
        opts.PrimaryColor             ??= accent;
        opts.PrimaryColorDark         ??= accent;
        opts.OnPrimaryColor           ??= Colors.White;

        // Windows surface colors.
        opts.SurfaceColor             ??= Color.FromArgb("#FAFAFA");
        opts.SurfaceColorDark         ??= Color.FromArgb("#202020");
        opts.OnSurfaceColor           ??= Color.FromArgb("#1B1B1B");
        opts.OnSurfaceColorDark       ??= Color.FromArgb("#FFFFFF");
        opts.OnSurfaceVariantColor    ??= Color.FromArgb("#605E5C");
        opts.OnSurfaceVariantColorDark ??= Color.FromArgb("#A19F9D");

        // Fluent: no header band; calendar grid (not wheel); dialog-style (not bottom sheet).
        opts.ShowHeader        = false;
        opts.SheetCornerRadius = 8;
        opts.CancelLabel       = "Cancel";
        opts.ConfirmLabel      = "OK";
        opts.Presentation      = PickerPresentation.Dialog;
        opts.PickerStyle       = PickerStyle.Calendar;
    }

    // ── Windows accent color ──────────────────────────────────────────────────

    private static Color GetWindowsAccentColor()
    {
#if WINDOWS
        try
        {
            var ui = new Windows.UI.ViewManagement.UISettings();
            var c  = ui.GetColorValue(Windows.UI.ViewManagement.UIColorType.Accent);
            return Color.FromRgba(c.R, c.G, c.B, c.A);
        }
        catch { }
#endif
        return Color.FromArgb("#0078D4");
    }
}

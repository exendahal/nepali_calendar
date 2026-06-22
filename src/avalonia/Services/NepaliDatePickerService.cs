using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using NepaliDatePicker.Controls;
using NepaliDatePicker.Models;

namespace NepaliDatePicker.Services;

/// <inheritdoc />
public class NepaliDatePickerService : INepaliDatePickerService
{
    public NepaliDate Today => BsAdConverter.AdToBs(DateTime.Today);

    public async Task<NepaliDate?> ShowAsync(NepaliDate? initialDate = null, NepaliDatePickerOptions? options = null)
    {
        var owner = ResolveOwnerWindow();
        if (owner is not null)
            return await new NepaliDatePickerWindow(initialDate, options).ShowDialog<NepaliDate?>(owner);

        var (topLevel, overlayLayer) = ResolveOverlay();
        if (topLevel is null || overlayLayer is null)
            throw new InvalidOperationException(
                "No active window or overlay layer found. Ensure the application has a running lifetime.");

        return await NepaliPickerOverlay.ShowInOverlay(topLevel, overlayLayer, initialDate, options);
    }

    private static Window? ResolveOwnerWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            foreach (var w in desktop.Windows)
                if (w.IsActive) return w;
            return desktop.MainWindow;
        }
        return null;
    }

    private static (TopLevel? topLevel, OverlayLayer? overlayLayer) ResolveOverlay()
    {
        if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime { MainView: { } mainView })
        {
            var tl = TopLevel.GetTopLevel(mainView);
            var ol = tl is not null ? OverlayLayer.GetOverlayLayer(mainView) : null;
            return (tl, ol);
        }
        return (null, null);
    }
}

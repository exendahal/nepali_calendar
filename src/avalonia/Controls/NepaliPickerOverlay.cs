using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using NepaliDatePicker.Models;

namespace NepaliDatePicker.Controls;

/// <summary>
/// Shared helper that shows the picker as a dialog on desktop
/// or as a full-screen scrim overlay on mobile / web.
/// </summary>
internal static class NepaliPickerOverlay
{
    internal static async Task<NepaliDate?> ShowAsync(
        Visual anchor, NepaliDate? initial, NepaliDatePickerOptions? options)
    {
        var topLevel = TopLevel.GetTopLevel(anchor);
        if (topLevel is null) return null;

        if (topLevel is Window window)
            return await new NepaliDatePickerWindow(initial, options).ShowDialog<NepaliDate?>(window);

        var overlayLayer = OverlayLayer.GetOverlayLayer(anchor);
        if (overlayLayer is null) return null;

        return await ShowInOverlay(topLevel, overlayLayer, initial, options);
    }

    internal static async Task<NepaliDate?> ShowInOverlay(
        TopLevel topLevel,
        OverlayLayer overlayLayer,
        NepaliDate? initial,
        NepaliDatePickerOptions? options)
    {
        var tcs  = new TaskCompletionSource<NepaliDate?>();
        var view = new NepaliDatePickerView(initial, options);
        double r = options?.SheetCornerRadius ?? 12;

        var card = new Border
        {
            CornerRadius        = new CornerRadius(r),
            ClipToBounds        = true,
            MinWidth            = 300,
            MaxWidth            = 400,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment   = VerticalAlignment.Center,
            BoxShadow           = new BoxShadows(new BoxShadow
            {
                Blur    = 24,
                OffsetY = 8,
                Color   = Color.FromArgb(100, 0, 0, 0),
            }),
            Child = view,
        };

        // OverlayLayer behaves like a Canvas — it does not pass its size to children.
        // We must set explicit Width/Height on the scrim so it covers the full screen.
        var scrim = new Grid
        {
            Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
        };
        scrim.Children.Add(card);

        void SyncSize()
        {
            var cs = topLevel.ClientSize;
            if (cs.Width  > 0) scrim.Width  = cs.Width;
            if (cs.Height > 0) scrim.Height = cs.Height;
        }

        SyncSize();

        // Keep in sync with orientation changes / window resize
        void OnLayoutUpdated(object? s, EventArgs e) => SyncSize();
        overlayLayer.LayoutUpdated += OnLayoutUpdated;

        void Finish(NepaliDate? date)
        {
            overlayLayer.LayoutUpdated -= OnLayoutUpdated;
            overlayLayer.Children.Remove(scrim);
            tcs.TrySetResult(date);
        }

        view.Done      += (_, date) => Finish(date);
        view.Cancelled += (_, _)    => Finish(null);
        overlayLayer.Children.Add(scrim);
        SyncSize(); // re-apply after add, in case layout ran during Add

        return await tcs.Task;
    }
}

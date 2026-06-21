using NepaliDatePicker.Controls;
using NepaliDatePicker.Models;

namespace NepaliDatePicker;

internal class NepaliDatePickerPage
{
    private readonly TaskCompletionSource<NepaliDate?> _Tcs = new();
    private readonly Grid _SheetContainer;
    private readonly bool _IsDialog;
    private bool _Dismissing;

    internal View RootView { get; }
    public Task<NepaliDate?> Result => _Tcs.Task;

    public NepaliDatePickerPage(NepaliDate? initialDate = null, NepaliDatePickerOptions? options = null)
    {
        _IsDialog = (options?.Presentation ?? PickerPresentation.BottomSheet) == PickerPresentation.Dialog;

        var sheet = new NepaliDatePickerSheet(initialDate, options);
        sheet.Done      += async (_, date) => await DismissAsync(date);
        sheet.Cancelled += async (_, _)    => await DismissAsync(null);

        double cr = options?.SheetCornerRadius ?? 28;
        CornerRadius cornerRadius = _IsDialog
            ? new CornerRadius(cr)
            : new CornerRadius(cr, cr, 0, 0);

        var sheetFrame = new Border
        {
            StrokeShape     = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = cornerRadius },
            Stroke          = Colors.Transparent,
            StrokeThickness = 0,
            Padding         = 0,
            Content         = sheet,
        };

        Color surfaceLight = options?.SurfaceColor ?? Color.FromArgb("#FFFBFE");
        Color surfaceDark  = options?.SurfaceColorDark ?? options?.SurfaceColor ?? Color.FromArgb("#1C1B1F");
        sheetFrame.SetAppThemeColor(Border.BackgroundColorProperty, surfaceLight, surfaceDark);

        bool fullWidth = options?.FullWidth ?? false;

        // Compact mode (default): 320 dp width, centered on both Dialog and BottomSheet.
        // WidthRequest gives the inner star-column grid a concrete parent width to divide.
        // Full-width mode: fill the screen (Dialog adds 32 dp side margins; BottomSheet is edge-to-edge).
        _SheetContainer = new Grid
        {
            VerticalOptions   = _IsDialog ? LayoutOptions.Center : LayoutOptions.End,
            HorizontalOptions = fullWidth ? LayoutOptions.Fill : LayoutOptions.Center,
            WidthRequest      = fullWidth ? -1 : 320,
            Margin            = fullWidth && _IsDialog ? new Thickness(32, 0) : Thickness.Zero,
        };

        _SheetContainer.Add(sheetFrame);

        Grid rootGrid;
        if (_IsDialog)
        {
            rootGrid = new Grid { BackgroundColor = Color.FromArgb("#80000000") };
            var scrim = new BoxView { Color = Colors.Transparent };
            var tgr = new TapGestureRecognizer();
            tgr.Tapped += async (_, _) => await DismissAsync(null);
            scrim.GestureRecognizers.Add(tgr);
            rootGrid.Add(scrim);
            rootGrid.Add(_SheetContainer);
        }
        else
        {
            rootGrid = new Grid
            {
                BackgroundColor = Color.FromArgb("#80000000"),
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Star),
                    new RowDefinition(GridLength.Auto),
                }
            };
            var scrim = new BoxView { Color = Colors.Transparent };
            var tgr = new TapGestureRecognizer();
            tgr.Tapped += async (_, _) => await DismissAsync(null);
            scrim.GestureRecognizers.Add(tgr);
            Grid.SetRow(scrim, 0);
            rootGrid.Add(scrim);
            Grid.SetRow(_SheetContainer, 1);
            rootGrid.Add(_SheetContainer);
        }

        RootView = rootGrid;
    }

    internal async Task AnimateInAsync()
    {
        if (_IsDialog)
        {
            _SheetContainer.Opacity = 0;
            _SheetContainer.Scale   = 0.92;
            await Task.WhenAll(
                _SheetContainer.FadeToAsync(1, 220, Easing.CubicOut),
                _SheetContainer.ScaleToAsync(1.0, 220, Easing.CubicOut));
        }
        else
        {
            _SheetContainer.TranslationY = 600;
            await _SheetContainer.TranslateToAsync(0, 0, 300, Easing.CubicOut);
        }
    }

    private async Task DismissAsync(NepaliDate? result)
    {
        if (_Dismissing) return;
        _Dismissing = true;

        if (_IsDialog)
        {
            await Task.WhenAll(
                _SheetContainer.FadeToAsync(0, 180, Easing.CubicIn),
                _SheetContainer.ScaleToAsync(0.92, 180, Easing.CubicIn));
        }
        else
        {
            await _SheetContainer.TranslateToAsync(0, 600, 260, Easing.CubicIn);
        }

        _Tcs.TrySetResult(result);
    }
}

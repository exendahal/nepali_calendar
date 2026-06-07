using NepaliDatePicker.Controls;
using NepaliDatePicker.Models;

namespace NepaliDatePicker;

internal class NepaliDatePickerPage : ContentPage
{
    private readonly TaskCompletionSource<NepaliDate?> _tcs = new();
    private readonly Grid _sheetContainer;
    private readonly bool _isDialog;

    public Task<NepaliDate?> Result => _tcs.Task;

    public NepaliDatePickerPage(NepaliDate? initialDate = null, NepaliDatePickerOptions? options = null)
    {
        BackgroundColor = Colors.Transparent;
        Shell.SetNavBarIsVisible(this, false);

        _isDialog = (options?.Presentation ?? PickerPresentation.BottomSheet) == PickerPresentation.Dialog;

        var sheet = new NepaliDatePickerSheet(initialDate, options);
        sheet.Done      += async (_, date) => await DismissAsync(date);
        sheet.Cancelled += async (_, _)    => await DismissAsync(null);

        var scrim = new BoxView { Color = Color.FromArgb("#80000000") };

        double cr = options?.SheetCornerRadius ?? 28;
        CornerRadius cornerRadius = _isDialog
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

        _sheetContainer = new Grid
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions   = _isDialog ? LayoutOptions.Center : LayoutOptions.End,
            Margin            = _isDialog ? new Thickness(32, 0) : Thickness.Zero,
        };
        _sheetContainer.Add(sheetFrame);

        Grid rootGrid;
        if (_isDialog)
        {
            // Single full-screen cell; scrim fills everything, container floats centered on top.
            rootGrid = new Grid();
            rootGrid.Add(scrim);
            rootGrid.Add(_sheetContainer);
        }
        else
        {
            // Two-row layout so the sheet anchors to the bottom.
            rootGrid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Star),
                    new RowDefinition(GridLength.Auto),
                }
            };
            rootGrid.Add(scrim);
            Grid.SetRowSpan(scrim, 2);
            Grid.SetRow(_sheetContainer, 1);
            rootGrid.Add(_sheetContainer);
        }

        Content = rootGrid;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = AnimateIn();
    }

    protected override bool OnBackButtonPressed()
    {
        _ = DismissAsync(null);
        return true;
    }

    private async Task AnimateIn()
    {
        if (_isDialog)
        {
            _sheetContainer.Opacity = 0;
            _sheetContainer.Scale   = 0.92;
            await Task.WhenAll(
                _sheetContainer.FadeToAsync(1, 220, Easing.CubicOut),
                _sheetContainer.ScaleToAsync(1.0, 220, Easing.CubicOut));
        }
        else
        {
            _sheetContainer.TranslationY = 600;
            await _sheetContainer.TranslateToAsync(0, 0, 300, Easing.CubicOut);
        }
    }

    private bool _dismissing;

    private async Task DismissAsync(NepaliDate? result)
    {
        if (_dismissing) return;
        _dismissing = true;

        if (_isDialog)
        {
            await Task.WhenAll(
                _sheetContainer.FadeToAsync(0, 180, Easing.CubicIn),
                _sheetContainer.ScaleToAsync(0.92, 180, Easing.CubicIn));
        }
        else
        {
            await _sheetContainer.TranslateToAsync(0, 600, 260, Easing.CubicIn);
        }

        try
        {
            if (Navigation != null)
                await Navigation.PopModalAsync(false);
        }
        finally
        {
            _tcs.TrySetResult(result);
        }
    }
}

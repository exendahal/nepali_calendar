using NepaliDatePicker.Controls;
using NepaliDatePicker.Models;

namespace NepaliDatePicker;

internal class NepaliDatePickerPage
{
    private readonly TaskCompletionSource<NepaliDate?> _tcs = new();
    private readonly Grid _sheetContainer;
    private readonly bool _isDialog;
    private bool _dismissing;

    internal View RootView { get; }
    public Task<NepaliDate?> Result => _tcs.Task;

    public NepaliDatePickerPage(NepaliDate? initialDate = null, NepaliDatePickerOptions? options = null)
    {
        _isDialog = (options?.Presentation ?? PickerPresentation.BottomSheet) == PickerPresentation.Dialog;

        var sheet = new NepaliDatePickerSheet(initialDate, options);
        sheet.Done      += async (_, date) => await DismissAsync(date);
        sheet.Cancelled += async (_, _)    => await DismissAsync(null);

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
            rootGrid = new Grid { BackgroundColor = Color.FromArgb("#80000000") };
            rootGrid.Add(_sheetContainer);
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
            Grid.SetRow(_sheetContainer, 1);
            rootGrid.Add(_sheetContainer);
        }

        RootView = rootGrid;
    }

    internal async Task AnimateInAsync()
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

        _tcs.TrySetResult(result);
    }
}

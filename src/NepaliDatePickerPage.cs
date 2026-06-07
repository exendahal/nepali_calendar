using nepali_calendar_picker.Controls;
using nepali_calendar_picker.Models;

namespace nepali_calendar_picker
{
    internal class NepaliDatePickerPage : ContentPage
    {
        private readonly TaskCompletionSource<NepaliDate?> _Tcs = new();
        private readonly Grid _SheetContainer;
        private readonly bool _IsDialog;

        public Task<NepaliDate?> Result => _Tcs.Task;

        public NepaliDatePickerPage(NepaliDate? initialDate = null, NepaliDatePickerOptions? options = null)
        {
            BackgroundColor = Colors.Transparent;
            Shell.SetNavBarIsVisible(this, false);

            _IsDialog = (options?.Presentation ?? PickerPresentation.BottomSheet) == PickerPresentation.Dialog;

            var sheet = new NepaliDatePickerSheet(initialDate, options);
            sheet.Done += async (_, date) => await DismissAsync(date);
            sheet.Cancelled += async (_, _) => await DismissAsync(null);

            var scrim = new BoxView { Color = Color.FromArgb("#80000000") };

            double cr = options?.SheetCornerRadius ?? 28;
            CornerRadius cornerRadius = _IsDialog
                ? new CornerRadius(cr)
                : new CornerRadius(cr, cr, 0, 0);

            var sheetFrame = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = cornerRadius },
                Stroke = Colors.Transparent,
                StrokeThickness = 0,
                Padding = 0,
                Content = sheet,
            };

            Color surfaceLight = options?.SurfaceColor ?? Color.FromArgb("#FFFBFE");
            Color surfaceDark = options?.SurfaceColorDark ?? options?.SurfaceColor ?? Color.FromArgb("#1C1B1F");
            sheetFrame.SetAppThemeColor(Border.BackgroundColorProperty, surfaceLight, surfaceDark);

            _SheetContainer = new Grid
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = _IsDialog ? LayoutOptions.Center : LayoutOptions.End,
                Margin = _IsDialog ? new Thickness(32, 0) : Thickness.Zero,
            };
            _SheetContainer.Add(sheetFrame);

            Grid rootGrid;
            if (_IsDialog)
            {
                // Single full-screen cell; scrim fills everything, container floats centered on top.
                rootGrid = new Grid();
                rootGrid.Add(scrim);
                rootGrid.Add(_SheetContainer);
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
                Grid.SetRow(_SheetContainer, 1);
                rootGrid.Add(_SheetContainer);
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
            if (_IsDialog)
            {
                _SheetContainer.Opacity = 0;
                _SheetContainer.Scale = 0.92;
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

        private bool _dismissing;

        private async Task DismissAsync(NepaliDate? result)
        {
            if (_dismissing) return;
            _dismissing = true;

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

            try
            {
                if (Navigation != null)
                    await Navigation.PopModalAsync(false);
            }
            finally
            {
                _Tcs.TrySetResult(result);
            }
        }
    }
}

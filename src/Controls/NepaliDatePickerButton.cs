using Microsoft.Maui.Controls.Shapes;
using NepaliDatePicker.Models;
using NepaliDatePicker.Services;

using NepaliDatePickerSvc = NepaliDatePicker.NepaliDatePickerService;

namespace NepaliDatePicker.Controls;

/// <summary>
/// A drop-in bindable control that looks like a date input field.
/// Tap it to open the Nepali (BS) date picker bottom sheet.
/// Bind <see cref="SelectedDate"/> two-way to your view model.
/// </summary>
public class NepaliDatePicker : ContentView
{
    // ── Bindable: core ────────────────────────────────────────────────────────

    public static readonly BindableProperty SelectedDateProperty =
        BindableProperty.Create(nameof(SelectedDate), typeof(NepaliDate), typeof(NepaliDatePicker), null,
            BindingMode.TwoWay,
            propertyChanged: (b, _, _) => ((NepaliDatePicker)b).Refresh());

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(NepaliDatePicker), "Select Date",
            propertyChanged: (b, _, _) => ((NepaliDatePicker)b).Refresh());

    public static readonly BindableProperty FormatProperty =
        BindableProperty.Create(nameof(Format), typeof(string), typeof(NepaliDatePicker), "d MMMM yyyy",
            propertyChanged: (b, _, _) => ((NepaliDatePicker)b).Refresh());

    public static readonly BindableProperty IsReadOnlyProperty =
        BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(NepaliDatePicker), false);

    // ── Bindable: display mode ────────────────────────────────────────────────

    /// <summary>Controls which calendar system(s) are available in the picker and what the button displays.</summary>
    public static readonly BindableProperty DisplayModeProperty =
        BindableProperty.Create(nameof(DisplayMode), typeof(DateDisplayMode), typeof(NepaliDatePicker),
            DateDisplayMode.Both,
            propertyChanged: (b, _, _) => ((NepaliDatePicker)b).Refresh());

    /// <summary>
    /// How the picker is presented.
    /// <see cref="PickerPresentation.BottomSheet"/> slides up from the bottom (default).
    /// <see cref="PickerPresentation.Dialog"/> floats centered over the page.
    /// </summary>
    public static readonly BindableProperty PresentationProperty =
        BindableProperty.Create(nameof(Presentation), typeof(PickerPresentation), typeof(NepaliDatePicker),
            PickerPresentation.BottomSheet);

    // ── Bindable: picker palette ──────────────────────────────────────────────

    /// <summary>Primary accent color passed to the picker (selected day, chips, buttons, today outline).</summary>
    public static readonly BindableProperty PrimaryColorProperty =
        BindableProperty.Create(nameof(PrimaryColor), typeof(Color), typeof(NepaliDatePicker), null);

    /// <summary>Primary color for dark theme in the picker.</summary>
    public static readonly BindableProperty PrimaryColorDarkProperty =
        BindableProperty.Create(nameof(PrimaryColorDark), typeof(Color), typeof(NepaliDatePicker), null);

    /// <summary>Custom font family used inside the picker.</summary>
    public static readonly BindableProperty PickerFontFamilyProperty =
        BindableProperty.Create(nameof(PickerFontFamily), typeof(string), typeof(NepaliDatePicker), null);

    /// <summary>Header band background color passed to the picker.</summary>
    public static readonly BindableProperty PickerHeaderColorProperty =
        BindableProperty.Create(nameof(PickerHeaderColor), typeof(Color), typeof(NepaliDatePicker), null);

    // ── Bindable: button appearance ───────────────────────────────────────────

    /// <summary>Border (stroke) color of the date input field in light theme.</summary>
    public static readonly BindableProperty BorderColorProperty =
        BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(NepaliDatePicker),
            Color.FromArgb("#C8C8C8"),
            propertyChanged: (b, _, _) => ((NepaliDatePicker)b).RefreshStyle());

    /// <summary>Border (stroke) color of the date input field in dark theme.</summary>
    public static readonly BindableProperty BorderColorDarkProperty =
        BindableProperty.Create(nameof(BorderColorDark), typeof(Color), typeof(NepaliDatePicker),
            Color.FromArgb("#3A3A3A"),
            propertyChanged: (b, _, _) => ((NepaliDatePicker)b).RefreshStyle());

    /// <summary>Background of the date input field in light theme.</summary>
    public static readonly BindableProperty InputBackgroundColorProperty =
        BindableProperty.Create(nameof(InputBackgroundColor), typeof(Color), typeof(NepaliDatePicker),
            Color.FromArgb("#F2F2F7"),
            propertyChanged: (b, _, _) => ((NepaliDatePicker)b).RefreshStyle());

    /// <summary>Background of the date input field in dark theme.</summary>
    public static readonly BindableProperty InputBackgroundColorDarkProperty =
        BindableProperty.Create(nameof(InputBackgroundColorDark), typeof(Color), typeof(NepaliDatePicker),
            Color.FromArgb("#2C2C2E"),
            propertyChanged: (b, _, _) => ((NepaliDatePicker)b).RefreshStyle());

    /// <summary>Date text color in light theme (when a date is selected).</summary>
    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(NepaliDatePicker),
            Color.FromArgb("#111111"),
            propertyChanged: (b, _, _) => ((NepaliDatePicker)b).Refresh());

    /// <summary>Date text color in dark theme.</summary>
    public static readonly BindableProperty TextColorDarkProperty =
        BindableProperty.Create(nameof(TextColorDark), typeof(Color), typeof(NepaliDatePicker),
            Color.FromArgb("#EEEEEE"),
            propertyChanged: (b, _, _) => ((NepaliDatePicker)b).Refresh());

    /// <summary>Placeholder text color in light theme.</summary>
    public static readonly BindableProperty PlaceholderColorProperty =
        BindableProperty.Create(nameof(PlaceholderColor), typeof(Color), typeof(NepaliDatePicker),
            Color.FromArgb("#AAAAAA"),
            propertyChanged: (b, _, _) => ((NepaliDatePicker)b).Refresh());

    /// <summary>Placeholder text color in dark theme.</summary>
    public static readonly BindableProperty PlaceholderColorDarkProperty =
        BindableProperty.Create(nameof(PlaceholderColorDark), typeof(Color), typeof(NepaliDatePicker),
            Color.FromArgb("#666666"),
            propertyChanged: (b, _, _) => ((NepaliDatePicker)b).Refresh());

    /// <summary>Font family for the date / placeholder label in the button.</summary>
    public static readonly BindableProperty FontFamilyProperty =
        BindableProperty.Create(nameof(FontFamily), typeof(string), typeof(NepaliDatePicker), null,
            propertyChanged: (b, _, n) => ((NepaliDatePicker)b)._dateLabel.FontFamily = (string?)n);

    /// <summary>Font size for the date / placeholder label. Default: 15.</summary>
    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(nameof(FontSize), typeof(double), typeof(NepaliDatePicker), 15.0,
            propertyChanged: (b, _, n) => ((NepaliDatePicker)b)._dateLabel.FontSize = (double)n);

    /// <summary>Corner radius of the date input border. Default: 10.</summary>
    public static readonly BindableProperty CornerRadiusProperty =
        BindableProperty.Create(nameof(CornerRadius), typeof(double), typeof(NepaliDatePicker), 10.0,
            propertyChanged: (b, _, n) =>
            {
                var self = (NepaliDatePicker)b;
                if (self._containerShape is not null)
                    self._containerShape.CornerRadius = new CornerRadius((double)n);
            });

    /// <summary>Calendar icon shown on the right of the input. Default: 📅</summary>
    public static readonly BindableProperty CalendarIconProperty =
        BindableProperty.Create(nameof(CalendarIcon), typeof(string), typeof(NepaliDatePicker), "📅",
            propertyChanged: (b, _, n) => ((NepaliDatePicker)b)._iconLabel.Text = (string?)n ?? "📅");

    /// <summary>
    /// When <c>true</c>, the selected date on the button and inside the picker is rendered
    /// in Devanagari script (Nepali numerals and month names). AD dates stay in English.
    /// </summary>
    public static readonly BindableProperty UseNepaliScriptProperty =
        BindableProperty.Create(nameof(UseNepaliScript), typeof(bool), typeof(NepaliDatePicker), false,
            propertyChanged: (b, _, _) => ((NepaliDatePicker)b).Refresh());

    // ── Public property accessors ─────────────────────────────────────────────

    public NepaliDate? SelectedDate
    {
        get => (NepaliDate?)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>Format tokens: d/dd=day, M/MM=month number, MMMM=month name, yy/yyyy=year.</summary>
    public string Format
    {
        get => (string)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public DateDisplayMode DisplayMode
    {
        get => (DateDisplayMode)GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    public PickerPresentation Presentation
    {
        get => (PickerPresentation)GetValue(PresentationProperty);
        set => SetValue(PresentationProperty, value);
    }

    public Color? PrimaryColor
    {
        get => (Color?)GetValue(PrimaryColorProperty);
        set => SetValue(PrimaryColorProperty, value);
    }

    public Color? PrimaryColorDark
    {
        get => (Color?)GetValue(PrimaryColorDarkProperty);
        set => SetValue(PrimaryColorDarkProperty, value);
    }

    public string? PickerFontFamily
    {
        get => (string?)GetValue(PickerFontFamilyProperty);
        set => SetValue(PickerFontFamilyProperty, value);
    }

    public Color? PickerHeaderColor
    {
        get => (Color?)GetValue(PickerHeaderColorProperty);
        set => SetValue(PickerHeaderColorProperty, value);
    }

    public Color BorderColor
    {
        get => (Color)GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }

    public Color BorderColorDark
    {
        get => (Color)GetValue(BorderColorDarkProperty);
        set => SetValue(BorderColorDarkProperty, value);
    }

    public Color InputBackgroundColor
    {
        get => (Color)GetValue(InputBackgroundColorProperty);
        set => SetValue(InputBackgroundColorProperty, value);
    }

    public Color InputBackgroundColorDark
    {
        get => (Color)GetValue(InputBackgroundColorDarkProperty);
        set => SetValue(InputBackgroundColorDarkProperty, value);
    }

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public Color TextColorDark
    {
        get => (Color)GetValue(TextColorDarkProperty);
        set => SetValue(TextColorDarkProperty, value);
    }

    public Color PlaceholderColor
    {
        get => (Color)GetValue(PlaceholderColorProperty);
        set => SetValue(PlaceholderColorProperty, value);
    }

    public Color PlaceholderColorDark
    {
        get => (Color)GetValue(PlaceholderColorDarkProperty);
        set => SetValue(PlaceholderColorDarkProperty, value);
    }

    public string? FontFamily
    {
        get => (string?)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public string CalendarIcon
    {
        get => (string)GetValue(CalendarIconProperty);
        set => SetValue(CalendarIconProperty, value);
    }

    public bool UseNepaliScript
    {
        get => (bool)GetValue(UseNepaliScriptProperty);
        set => SetValue(UseNepaliScriptProperty, value);
    }

    // ── Events ────────────────────────────────────────────────────────────────
    public event EventHandler<NepaliDate?>? DateSelected;

    // ── Private state ─────────────────────────────────────────────────────────
    private readonly Label _dateLabel;
    private readonly Label _iconLabel;
    private readonly Border _container;
    private readonly RoundRectangle _containerShape;
    private bool _isPickerOpen;

    public NepaliDatePicker()
    {
        _dateLabel = new Label
        {
            VerticalTextAlignment   = TextAlignment.Center,
            HorizontalTextAlignment = TextAlignment.Start,
            FontSize  = 15,
            HorizontalOptions = LayoutOptions.Fill,
        };

        _iconLabel = new Label
        {
            Text = "📅",
            FontSize = 16,
            VerticalTextAlignment = TextAlignment.Center,
        };

        _containerShape = new RoundRectangle { CornerRadius = 10 };

        _container = new Border
        {
            Padding         = new Thickness(14, 0),
            HeightRequest   = 48,
            StrokeShape     = _containerShape,
            StrokeThickness = 1,
            Content         = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                },
                Children = { _dateLabel, _iconLabel }
            }
        };
        Grid.SetColumn(_iconLabel, 1);

        RefreshStyle();

        var tap = new TapGestureRecognizer();
        tap.Tapped += OnTapped;
        _container.GestureRecognizers.Add(tap);

        Content = _container;
        Refresh();
    }

    // ── Interactions ──────────────────────────────────────────────────────────

    private async void OnTapped(object? sender, TappedEventArgs e)
    {
        if (IsReadOnly || _isPickerOpen) return;
        _isPickerOpen = true;

        try
        {
            var opts = BuildPickerOptions();
            var service = new NepaliDatePickerSvc();
            var result = await service.ShowAsync(SelectedDate, opts);
            if (result is not null)
            {
                SelectedDate = result;
                DateSelected?.Invoke(this, result);
            }
        }
        finally
        {
            _isPickerOpen = false;
        }
    }

    private NepaliDatePickerOptions BuildPickerOptions()
    {
        var opts = new NepaliDatePickerOptions
        {
            DisplayMode     = DisplayMode,
            FontFamily      = PickerFontFamily,
            Presentation    = Presentation,
            UseNepaliScript = UseNepaliScript,
        };

        if (PrimaryColor is not null)      opts.PrimaryColor          = PrimaryColor;
        if (PrimaryColorDark is not null)  opts.PrimaryColorDark      = PrimaryColorDark;
        if (PickerHeaderColor is not null)  opts.HeaderBackgroundColor = PickerHeaderColor;

        return opts;
    }

    // ── Display ───────────────────────────────────────────────────────────────

    private void Refresh()
    {
        if (SelectedDate is null)
        {
            _dateLabel.Text = Placeholder;
            _dateLabel.SetAppThemeColor(Label.TextColorProperty, PlaceholderColor, PlaceholderColorDark);
        }
        else
        {
            _dateLabel.Text = FormatDate(SelectedDate);
            _dateLabel.SetAppThemeColor(Label.TextColorProperty, TextColor, TextColorDark);
        }
    }

    private void RefreshStyle()
    {
        _container.SetAppThemeColor(Border.StrokeProperty,            BorderColor,          BorderColorDark);
        _container.SetAppThemeColor(Border.BackgroundColorProperty,   InputBackgroundColor, InputBackgroundColorDark);
    }

    private string FormatDate(NepaliDate bsDate)
    {
        if (DisplayMode == DateDisplayMode.AdOnly)
        {
            DateTime ad = BsAdConverter.BsToAd(bsDate);
            return Format
                .Replace("MMMM", ad.ToString("MMMM"))
                .Replace("MMM",  ad.ToString("MMM"))
                .Replace("MM",   ad.Month.ToString("D2"))
                .Replace("M",    ad.Month.ToString())
                .Replace("yyyy", ad.Year.ToString())
                .Replace("yy",   (ad.Year % 100).ToString("D2"))
                .Replace("dd",   ad.Day.ToString("D2"))
                .Replace("d",    ad.Day.ToString());
        }

        if (UseNepaliScript)
        {
            return Format
                .Replace("MMMM", bsDate.MonthNameNepali)
                .Replace("MMM",  bsDate.MonthNameNepali)
                .Replace("MM",   Nep(bsDate.Month, pad: true))
                .Replace("M",    Nep(bsDate.Month))
                .Replace("yyyy", Nep(bsDate.Year))
                .Replace("yy",   Nep(bsDate.Year % 100, pad: true))
                .Replace("dd",   Nep(bsDate.Day, pad: true))
                .Replace("d",    Nep(bsDate.Day));
        }

        return Format
            .Replace("MMMM", bsDate.MonthName)
            .Replace("MMM",  bsDate.MonthName[..3])
            .Replace("MM",   bsDate.Month.ToString("D2"))
            .Replace("M",    bsDate.Month.ToString())
            .Replace("yyyy", bsDate.Year.ToString())
            .Replace("yy",   (bsDate.Year % 100).ToString("D2"))
            .Replace("dd",   bsDate.Day.ToString("D2"))
            .Replace("d",    bsDate.Day.ToString());
    }

    private static string Nep(int n, bool pad = false)
    {
        var s = pad ? n.ToString("D2") : n.ToString();
        return s
            .Replace('0', '०').Replace('1', '१').Replace('2', '२')
            .Replace('3', '३').Replace('4', '४').Replace('5', '५')
            .Replace('6', '६').Replace('7', '७').Replace('8', '८')
            .Replace('9', '९');
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using NepaliDatePicker.Models;
using NepaliUtility.Models;

namespace NepaliDatePicker.Controls;

/// <summary>
/// A text-field-style date input that opens the Nepali date picker on tap/click.
/// Shows a placeholder when empty; fills with the selected BS date when a date is chosen.
/// Works on desktop (dialog) and mobile / web (overlay).
/// </summary>
public class NepaliDatePickerField : UserControl
{
    // ── Styled properties ─────────────────────────────────────────────────────

    public static readonly StyledProperty<NepaliDate?> SelectedDateProperty =
        AvaloniaProperty.Register<NepaliDatePickerField, NepaliDate?>(nameof(SelectedDate));

    public static readonly StyledProperty<NepaliDatePickerOptions?> PickerOptionsProperty =
        AvaloniaProperty.Register<NepaliDatePickerField, NepaliDatePickerOptions?>(nameof(PickerOptions));

    public static readonly StyledProperty<string> PlaceholderTextProperty =
        AvaloniaProperty.Register<NepaliDatePickerField, string>(nameof(PlaceholderText), "Select a date");

    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<NepaliDatePickerField, string?>(nameof(Label));

    public NepaliDate? SelectedDate
    {
        get => GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    public NepaliDatePickerOptions? PickerOptions
    {
        get => GetValue(PickerOptionsProperty);
        set => SetValue(PickerOptionsProperty, value);
    }

    public string PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    // ── Events ────────────────────────────────────────────────────────────────

    public event EventHandler<NepaliDate>? DateSelected;

    // ── Internal UI refs ──────────────────────────────────────────────────────

    private readonly TextBlock _DateText;
    private readonly Border    _FieldBorder;
    private readonly TextBlock _LabelText;

    // ── Border colors ─────────────────────────────────────────────────────────

    private static readonly Color _OutlineLight = Color.Parse("#79747E");
    private static readonly Color _OutlineDark  = Color.Parse("#CAC4D0");

    private IBrush DefaultOutline =>
        ActualThemeVariant == ThemeVariant.Dark
            ? new SolidColorBrush(_OutlineDark)
            : new SolidColorBrush(_OutlineLight);

    // ── Constructor ───────────────────────────────────────────────────────────

    public NepaliDatePickerField()
    {
        _DateText = new TextBlock
        {
            FontSize          = 15,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var icon = new TextBlock
        {
            Text              = "📅",
            FontSize          = 17,
            VerticalAlignment = VerticalAlignment.Center,
            Margin            = new Thickness(8, 0, 0, 0),
        };

        var row = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*, Auto"),
            Background        = Brushes.Transparent, // full-width hit testing
        };
        row.Children.Add(_DateText);
        Grid.SetColumn(icon, 1);
        row.Children.Add(icon);

        _FieldBorder = new Border
        {
            CornerRadius    = new CornerRadius(8),
            BorderThickness = new Thickness(1.5),
            Padding         = new Thickness(14, 12),
            Cursor          = new Cursor(StandardCursorType.Hand),
            Child           = row,
        };

        _LabelText = new TextBlock
        {
            FontSize  = 12,
            Opacity   = 0.65,
            Margin    = new Thickness(2, 0, 0, 5),
        };

        var container = new StackPanel { Spacing = 0 };
        container.Children.Add(_LabelText);
        container.Children.Add(_FieldBorder);
        Content = container;

        // React to property changes
        this.PropertyChanged += (_, e) =>
        {
            if (e.Property == LabelProperty)
                _LabelText.IsVisible = !string.IsNullOrEmpty(Label);

            if (e.Property == SelectedDateProperty ||
                e.Property == PickerOptionsProperty ||
                e.Property == PlaceholderTextProperty)
                UpdateDisplay(SelectedDate);
        };
        this.ActualThemeVariantChanged += (_, _) => UpdateDisplay(SelectedDate);

        // Open picker on tap / click
        _FieldBorder.PointerPressed += async (_, _) =>
        {
            var result = await NepaliPickerOverlay.ShowAsync(this, SelectedDate, PickerOptions);
            if (result != null)
            {
                SelectedDate = result;
                DateSelected?.Invoke(this, result);
            }
        };

        _LabelText.IsVisible = false;
        UpdateDisplay(null);
    }

    // ── Display logic ─────────────────────────────────────────────────────────

    private void UpdateDisplay(NepaliDate? date)
    {
        var primary = PickerOptions?.PrimaryColor ?? Color.Parse("#6750A4");

        if (date is null)
        {
            _DateText.Text           = PlaceholderText;
            _DateText.Opacity        = 0.45;
            _FieldBorder.BorderBrush = DefaultOutline;
        }
        else
        {
            _DateText.Text           = date.ToDisplayString();
            _DateText.Opacity        = 1.0;
            _FieldBorder.BorderBrush = new SolidColorBrush(primary);
        }
    }
}

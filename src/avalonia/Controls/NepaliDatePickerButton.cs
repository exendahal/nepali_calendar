using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using NepaliDatePicker.Models;
using NepaliUtility.Models;

namespace NepaliDatePicker.Controls;

/// <summary>
/// A button that opens the Nepali date picker and exposes the selected date.
/// Works on desktop (dialog) and mobile/web (overlay).
/// </summary>
public class NepaliDatePickerButton : UserControl
{
    public static readonly StyledProperty<NepaliDate?> SelectedDateProperty =
        AvaloniaProperty.Register<NepaliDatePickerButton, NepaliDate?>(nameof(SelectedDate));

    public static readonly StyledProperty<NepaliDatePickerOptions?> PickerOptionsProperty =
        AvaloniaProperty.Register<NepaliDatePickerButton, NepaliDatePickerOptions?>(nameof(PickerOptions));

    public static readonly StyledProperty<string> ButtonTextProperty =
        AvaloniaProperty.Register<NepaliDatePickerButton, string>(nameof(ButtonText), "Select Date");

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

    public string ButtonText
    {
        get => GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }

    public event EventHandler<NepaliDate>? DateSelected;

    public NepaliDatePickerButton()
    {
        var btn = new Button { HorizontalAlignment = HorizontalAlignment.Stretch };
        btn.Bind(Button.ContentProperty, this.GetObservable(ButtonTextProperty));
        btn.Click += async (_, _) =>
        {
            var result = await NepaliPickerOverlay.ShowAsync(this, SelectedDate, PickerOptions);
            if (result != null)
            {
                SelectedDate = result;
                DateSelected?.Invoke(this, result);
            }
        };
        Content = btn;
    }
}

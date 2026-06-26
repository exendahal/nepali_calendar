using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using NepaliDatePicker.Models;
using NepaliUtility.Models;

namespace NepaliDatePicker.Controls;

/// <summary>
/// Dialog window that hosts <see cref="NepaliDatePickerView"/> and returns the selected date.
/// </summary>
public class NepaliDatePickerWindow : Window
{
    public NepaliDatePickerWindow(NepaliDate? initial = null, NepaliDatePickerOptions? options = null)
    {
        Title            = string.Empty;
        ShowInTaskbar    = false;
        CanResize        = false;
        SizeToContent    = SizeToContent.WidthAndHeight;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        double radius = options?.SheetCornerRadius ?? 12;
        CornerRadius  = new CornerRadius(radius);

        var view = new NepaliDatePickerView(initial, options);
        view.Done      += (_, date) => Close(date);
        view.Cancelled += (_, _)    => Close(null);

        Content = new Border
        {
            CornerRadius = new CornerRadius(radius),
            ClipToBounds = true,
            MinWidth     = 320,
            MaxWidth     = 420,
            Child        = view,
        };
    }
}

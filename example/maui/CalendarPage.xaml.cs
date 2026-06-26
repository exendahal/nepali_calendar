using NepaliUtility.Models;

namespace NepaliDatePickerDemo;

public partial class CalendarPage : ContentPage
{
    public CalendarPage()
    {
        InitializeComponent();
    }

    private void OnDateSelected(object sender, NepaliDate? date)
    {
        SelectedDateLabel.Text = date is not null
            ? date.ToDisplayString()
            : "No date selected";
    }
}

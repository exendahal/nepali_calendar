using NepaliDatePicker.Models;

namespace NepaliDatePickerDemo;

public partial class MainPage : ContentPage
{
    // DI injects MainViewModel (which has INepaliDatePickerService injected into it)
    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    private void OnDateSelected(object sender, NepaliDate? date)
    {
        if (date is not null && BindingContext is MainViewModel vm)
            vm.StatusMessage = $"Control selected: {date.ToDisplayString()}";
    }

    private async void OnOpenCalendarPageClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CalendarPage());
    }

    private async void OnOpenUtilsPageClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new UtilsPage());
    }
}

using NepaliDatePicker.Models;
using NepaliDatePicker.Services;

namespace NepaliDatePicker;

internal sealed class NepaliDatePickerService : INepaliDatePickerService
{
    public NepaliDate Today => BsAdConverter.AdToBs(DateTime.Today);

    public async Task<NepaliDate?> ShowAsync(NepaliDate? initialDate = null, NepaliDatePickerOptions? options = null)
    {
        var page = GetCurrentPage() ?? throw new InvalidOperationException(
            "NepaliDatePicker: No active Page found. " +
            "Ensure ShowAsync() is called while a page is displayed.");

        var pickerPage = new NepaliDatePickerPage(initialDate ?? Today, options);
        await page.Navigation.PushModalAsync(pickerPage, animated: false);
        return await pickerPage.Result;
    }

    private static Page? GetCurrentPage()
    {
        var root = Application.Current?.Windows.FirstOrDefault()?.Page;
        return root switch
        {
            Shell shell        => shell.CurrentPage,
            NavigationPage nav => nav.CurrentPage,
            TabbedPage tabbed  => tabbed.CurrentPage,
            FlyoutPage flyout  => flyout.Detail,
            _                  => root,
        };
    }
}

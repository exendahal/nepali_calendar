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

        if (page is not ContentPage contentPage)
            throw new InvalidOperationException(
                "NepaliDatePicker: ShowAsync requires a ContentPage as the current page.");

        var picker = new NepaliDatePickerPage(initialDate ?? Today, options);

        // Snapshot the page's current content, then layer the picker on top in a fresh Grid.
        // A fresh Grid is created on every open so there is no stale layout state between uses.
        var originalContent = contentPage.Content;
        var overlay = new Grid();
        if (originalContent is not null)
            overlay.Add(originalContent);
        overlay.Add(picker.RootView);
        contentPage.Content = overlay;

        await picker.AnimateInAsync();
        var result = await picker.Result;

        // Restore the original page content so the next open starts from a clean state.
        if (originalContent is not null)
            overlay.Remove(originalContent);
        contentPage.Content = originalContent;

        return result;
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

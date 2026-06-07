using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NepaliDatePicker.Models;
using NepaliDatePicker.Services;

namespace NepaliDatePickerDemo;

public partial class MainViewModel : ObservableObject
{
    private readonly INepaliDatePickerService _picker;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedDateDisplay))]
    [NotifyPropertyChangedFor(nameof(AdEquivalentDisplay))]
    private NepaliDate? _selectedDate;

    [ObservableProperty]
    private string _statusMessage = "Tap any input or button below to open the picker.";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StartDateDisplay))]
    [NotifyPropertyChangedFor(nameof(RangeDurationDisplay))]
    private NepaliDate? _startDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EndDateDisplay))]
    [NotifyPropertyChangedFor(nameof(RangeDurationDisplay))]
    private NepaliDate? _endDate;

    // ── Range displays ────────────────────────────────────────────────────────
    public string StartDateDisplay =>
        StartDate is null ? "Not set" : StartDate.ToDisplayString();

    public string EndDateDisplay =>
        EndDate is null ? "Not set" : EndDate.ToDisplayString();

    public string RangeDurationDisplay
    {
        get
        {
            if (StartDate is null || EndDate is null) return "—";
            var startAd = BsAdConverter.BsToAd(StartDate);
            var endAd   = BsAdConverter.BsToAd(EndDate);
            int days    = (int)(endAd - startAd).TotalDays;
            return days >= 0 ? $"{days} day{(days == 1 ? "" : "s")}" : "Invalid range (end before start)";
        }
    }

    // ── Today displays ────────────────────────────────────────────────────────
    public string TodayBsDisplay => _picker.Today.ToDisplayString();
    public string TodayAdDisplay => $"AD: {BsAdConverter.FormatAdEquivalent(_picker.Today)}";

    // ── Selected date displays ────────────────────────────────────────────────
    public string SelectedDateDisplay =>
        SelectedDate is null ? "No date selected" : SelectedDate.ToDisplayString();

    public string AdEquivalentDisplay =>
        SelectedDate is null ? "—" : $"AD: {BsAdConverter.FormatAdEquivalent(SelectedDate)}";

    public MainViewModel(INepaliDatePickerService picker)
    {
        _picker = picker;
        SelectedDate = picker.Today;
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task OpenPickerAsync()
    {
        var result = await _picker.ShowAsync(SelectedDate);
        if (result is not null)
        {
            SelectedDate = result;
            StatusMessage = $"Selected via API: {result.ToDisplayString()}";
        }
        else
        {
            StatusMessage = "Picker cancelled.";
        }
    }

    [RelayCommand]
    private async Task OpenPickerAtSpecificDateAsync()
    {
        var seed = new NepaliDate(2075, 6, 15);
        var result = await _picker.ShowAsync(seed);
        if (result is not null)
        {
            SelectedDate = result;
            StatusMessage = $"Selected: {result.ToDisplayString()} (started at 2075 Ashwin 15)";
        }
    }

    [RelayCommand]
    private async Task OpenPickerBsOnlyAsync()
    {
        var opts = new NepaliDatePickerOptions
        {
            DisplayMode       = DateDisplayMode.BsOnly,
            PrimaryColor      = Color.FromArgb("#00897B"),
            PrimaryColorDark  = Color.FromArgb("#80CBC4"),
            HeaderBackgroundColor = Color.FromArgb("#00695C"),
            SheetCornerRadius = 20,
        };
        var result = await _picker.ShowAsync(SelectedDate, opts);
        if (result is not null)
        {
            SelectedDate = result;
            StatusMessage = $"Selected (custom teal BS-only): {result.ToDisplayString()}";
        }
    }

    [RelayCommand]
    private void ClearDate()
    {
        SelectedDate = null;
        StatusMessage = "Date cleared.";
    }

    [RelayCommand]
    private void SetToday()
    {
        SelectedDate = _picker.Today;
        StatusMessage = $"Set to today: {SelectedDate?.ToDisplayString()}";
    }

    [RelayCommand]
    private async Task OpenRangePickerAsync()
    {
        var start = await _picker.ShowAsync(StartDate);
        if (start is null) return;

        var end = await _picker.ShowAsync(EndDate ?? start);
        if (end is null) return;

        StartDate = start;
        EndDate   = end;
        StatusMessage = $"Range: {start.ToDisplayString()} → {end.ToDisplayString()}";
    }

    [RelayCommand]
    private async Task OpenDialogPickerAsync()
    {
        var opts = new NepaliDatePickerOptions
        {
            Presentation = PickerPresentation.Dialog,
        };
        var result = await _picker.ShowAsync(SelectedDate, opts);
        if (result is not null)
        {
            SelectedDate = result;
            StatusMessage = $"Selected via dialog: {result.ToDisplayString()}";
        }
        else
        {
            StatusMessage = "Dialog picker cancelled.";
        }
    }
}

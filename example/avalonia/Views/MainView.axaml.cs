using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using NepaliDatePicker.Models;
using NepaliDatePicker.Services;

namespace NepaliDatePickerDemo.Avalonia.Views;

public partial class MainView : UserControl
{
    private readonly NepaliDatePickerService _Service = new();

    private bool _UseNepali    = false;
    private PickerStyle _Style = PickerStyle.Calendar;
    private NepaliDate? _StartDate;
    private NepaliDate? _EndDate;

    private static readonly Color _Purple = Color.Parse("#6750A4");
    private static readonly Color _Teal   = Color.Parse("#00695C");
    private static readonly Color _Coral  = Color.Parse("#E53935");

    public MainView()
    {
        InitializeComponent();
        SetTodayBanner();
        UpdatePickerOptions();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // Apply system bar insets so content doesn't sit under the status bar
        // (needed on Android 15+ where edge-to-edge is mandatory, and iOS).
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.InsetsManager is { } mgr)
        {
            ApplySafeArea(mgr.SafeAreaPadding);
            mgr.SafeAreaChanged += (_, args) => ApplySafeArea(args.SafeAreaPadding);
        }
    }

    private void ApplySafeArea(Thickness insets) =>
        RootScroll.Padding = new Thickness(0, Math.Max(insets.Top, 48), 0, insets.Bottom);

    private void SetTodayBanner()
    {
        var today = BsAdConverter.AdToBs(DateTime.Today);
        TodayBsLabel.Text = $"BS {today.Year}-{today.Month:D2}-{today.Day:D2}";
        TodayAdLabel.Text = DateTime.Today.ToString("d MMMM yyyy");
    }

    private NepaliDatePickerOptions MakeOptions(Color? primary = null, Color? header = null) =>
        new()
        {
            UseNepaliScript       = _UseNepali,
            PickerStyle           = _Style,
            PrimaryColor          = primary,
            HeaderBackgroundColor = header,
        };

    private void UpdatePickerOptions()
    {
        DefaultPickerField.PickerOptions = MakeOptions();
        TealPickerField.PickerOptions    = MakeOptions(_Teal,  _Teal);
        CoralPickerField.PickerOptions   = MakeOptions(_Coral, _Coral);
        StartDateField.PickerOptions     = MakeOptions();
        EndDateField.PickerOptions       = MakeOptions();
    }

    // ── Chip helpers ───────────────────────────────────────────────────────────

    private static readonly SolidColorBrush _PurpleBrush = new(Color.Parse("#6750A4"));

    private static void SetChipActive(Border chip, TextBlock label, bool active)
    {
        chip.Background      = active ? _PurpleBrush : Brushes.Transparent;
        chip.BorderThickness = active ? new Thickness(0) : new Thickness(1.5);
        label.Foreground     = active ? Brushes.White  : _PurpleBrush;
    }

    // ── Configuration chip handlers ────────────────────────────────────────────

    private void OnEnglishChipPressed(object? sender, PointerPressedEventArgs e) => SetLanguage(false);
    private void OnNepaliChipPressed(object? sender, PointerPressedEventArgs e)  => SetLanguage(true);
    private void OnCalendarChipPressed(object? sender, PointerPressedEventArgs e) => SetStyle(PickerStyle.Calendar);
    private void OnInputChipPressed(object? sender, PointerPressedEventArgs e)    => SetStyle(PickerStyle.Input);

    private void SetLanguage(bool nepali)
    {
        _UseNepali = nepali;
        SetChipActive(EnglishChip, EnglishChipText, !nepali);
        SetChipActive(NepaliChip,  NepaliChipText,   nepali);
        UpdatePickerOptions();
    }

    private void SetStyle(PickerStyle style)
    {
        _Style = style;
        bool isCalendar = style == PickerStyle.Calendar;
        SetChipActive(CalendarChip, CalendarChipText,  isCalendar);
        SetChipActive(InputChip,    InputChipText,     !isCalendar);
        UpdatePickerOptions();
    }

    // ── Picker results ─────────────────────────────────────────────────────────

    private void OnPickerDateSelected(object? sender, NepaliDate date)
    {
        ResultLabel.Text   = $"BS  {date.ToDisplayString()}";
        ResultAdLabel.Text = $"AD  {BsAdConverter.BsToAd(date):d MMMM yyyy}";
    }

    // ── Date range ─────────────────────────────────────────────────────────────

    private void OnStartDateSelected(object? sender, NepaliDate date)
    {
        _StartDate = date;
        UpdateDuration();
    }

    private void OnEndDateSelected(object? sender, NepaliDate date)
    {
        _EndDate = date;
        UpdateDuration();
    }

    private void UpdateDuration()
    {
        if (_StartDate is null || _EndDate is null) { DurationLabel.Text = "—"; return; }
        var start = BsAdConverter.BsToAd(_StartDate);
        var end   = BsAdConverter.BsToAd(_EndDate);
        int days  = (int)(end - start).TotalDays;
        DurationLabel.Text = days >= 0
            ? $"{days} day{(days == 1 ? "" : "s")}"
            : "End date is before start date";
    }

    // ── Color presets ──────────────────────────────────────────────────────────

    private async void OnColorSwatchPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border { Tag: string hex }) return;
        var color = Color.Parse(hex);
        NepaliDate? date;
        try
        {
            date = await _Service.ShowAsync(options: new NepaliDatePickerOptions
            {
                UseNepaliScript       = _UseNepali,
                PickerStyle           = _Style,
                PrimaryColor          = color,
                HeaderBackgroundColor = color,
            });
        }
        catch { return; }
        if (date is null) return;
        ColorResultLabel.Text   = $"BS  {date.ToDisplayString()}";
        ColorResultAdLabel.Text = $"AD  {BsAdConverter.BsToAd(date):d MMMM yyyy}";
    }
}

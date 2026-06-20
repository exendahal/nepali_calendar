using Microsoft.Maui.Controls.Shapes;
using NepaliDatePicker.Data;
using NepaliDatePicker.Models;
using NepaliDatePicker.Services;

namespace NepaliDatePicker.Controls;

/// <summary>
/// Material Design 3 calendar date-picker sheet.
/// Layout: drag handle → colored header → BS/AD chip toggle (optional) →
///         month navigation → day-of-week headers → calendar grid → CANCEL / OK actions.
/// Tapping the month/year label toggles a year + month grid for fast navigation.
/// With <see cref="PickerStyle.Wheel"/> the navigation and calendar grid are replaced
/// by three iOS-style drum-roll wheels (year / month / day).
/// </summary>
internal class NepaliDatePickerSheet : ContentView
{
    // ── Picker sub-mode ───────────────────────────────────────────────────────
    private enum PickerMode { Calendar, YearMonth }

    // ── MD3 static defaults ───────────────────────────────────────────────────
    private static readonly Color _Md3Primary        = Color.FromArgb("#6750A4");
    private static readonly Color _Md3PrimaryDark    = Color.FromArgb("#D0BCFF");
    private static readonly Color _Md3OnPrimary      = Colors.White;
    private static readonly Color _Md3OnPrimaryDark  = Color.FromArgb("#381E72");
    private static readonly Color _Md3Surface        = Color.FromArgb("#FFFBFE");
    private static readonly Color _Md3SurfaceDark    = Color.FromArgb("#1C1B1F");
    private static readonly Color _Md3OnSurface      = Color.FromArgb("#1C1B1F");
    private static readonly Color _Md3OnSurfaceDark  = Color.FromArgb("#E6E1E5");
    private static readonly Color _Md3OnSurfaceVar   = Color.FromArgb("#49454F");
    private static readonly Color _Md3OnSurfaceVarDk = Color.FromArgb("#CAC4D0");
    private static readonly Color _Md3HeaderBgLight  = Color.FromArgb("#6750A4");
    private static readonly Color _Md3HeaderBgDark   = Color.FromArgb("#4A4458");

    // ── Effective colors ──────────────────────────────────────────────────────
    private readonly Color _Primary, _PrimaryDark;
    private readonly Color _OnPrimary, _OnPrimaryDark;
    private readonly Color _HeaderBg, _HeaderBgDark;
    private readonly Color _HeaderText;
    private readonly Color _Surface, _SurfaceDark;
    private readonly Color _OnSurface, _OnSurfaceDark;
    private readonly Color _OnSurfaceVar, _OnSurfaceVarDk;
    private readonly string? _FontFamily;
    private readonly bool _UseNepaliScript;

    // ── Events ────────────────────────────────────────────────────────────────
    public event EventHandler<NepaliDate>? Done;
    public event EventHandler? Cancelled;

    // ── Selection state ───────────────────────────────────────────────────────
    private bool _IsBsMode;
    private int _BsYear, _BsMonth, _BsDay;
    private DateTime _AdDate;
    private readonly DateDisplayMode _DisplayMode;
    private readonly PickerStyle _PickerStyle;

    // ── View state ────────────────────────────────────────────────────────────
    private int _ViewYear, _ViewMonth;

    // ── Year/month picker state ───────────────────────────────────────────────
    private PickerMode _PickerMode = PickerMode.Calendar;
    private int _PickerSelectedYear;

    // ── Mutable UI references ─────────────────────────────────────────────────
    private readonly Label _HeaderDateLabel;
    private readonly Label _HeaderEquivLabel;
    private readonly Label _MonthYearLabel;
    private readonly Label _ChevronLabel;
    private readonly Button _PrevBtn;
    private readonly Button _NextBtn;
    private readonly Grid _DowRow;
    private readonly ContentView _CalendarHost;
    private readonly Border _BsChip, _AdChip;

    // ── Wheel-style state (created only for PickerStyle.Wheel) ────────────────
    private DrumRollPicker? _YearWheel, _MonthWheel, _DayWheel;
    private bool _WheelSyncing;     // guards wheel ⇄ selection feedback loops
    private int  _WheelMinYear;     // year represented by index 0 of the year wheel
    private int  _WheelDayCount;    // number of items currently in the day wheel

    // ── Input-style state (created only for PickerStyle.Input) ───────────────
    private Button? _OkBtn;
    private Entry?  _InputEntry;

    // Sun → Sat, matching DayOfWeek order (Sunday = 0)
    private static readonly string[] _DowLabels       = ["S",    "M",   "T",     "W",   "T",     "F",     "S"   ];
    private static readonly string[] _DowLabelsNepali = ["आइ",  "सो",  "मं",   "बु",  "बि",   "शु",   "श"   ];
    // Longer abbreviations used in the header date line (e.g. "आइत, बैशाख ५, २०८२")
    private static readonly string[] _DowHeaderNepali = ["आइत", "सोम", "मंगल", "बुध", "बिही", "शुक्र", "शनि"];
    private static readonly string[] _AdMonthNames =
    [
        "January","February","March","April","May","June",
        "July","August","September","October","November","December"
    ];

    // ── Layout constants for year/month picker ────────────────────────────────
    private const int _YearCellH    = 36;
    private const int _YearSpacing  = 2;
    private const int _YearCols     = 3;
    private const int _YearVisible  = 4;   // rows visible in scroll
    private const int _MonthCellH   = 44;
    private const int _MonthSpacing = 2;
    private const int _MonthCols    = 3;

    // Nepali script applies in BS mode, and also to the AD calendar when the
    // picker is AD-only (there is no BS view to carry the script in that case).
    private bool UseNepaliGlyphs => _UseNepaliScript && (_IsBsMode || _DisplayMode == DateDisplayMode.AdOnly);

    public NepaliDatePickerSheet(NepaliDate? initial = null, NepaliDatePickerOptions? options = null)
    {
        // ── Resolve effective colors ──────────────────────────────────────────
        _Primary        = options?.PrimaryColor                  ?? _Md3Primary;
        _PrimaryDark    = options?.PrimaryColorDark              ?? options?.PrimaryColor ?? _Md3PrimaryDark;
        _OnPrimary      = options?.OnPrimaryColor                ?? _Md3OnPrimary;
        _OnPrimaryDark  = options?.OnPrimaryColor                ?? _Md3OnPrimaryDark;
        _HeaderBg       = options?.HeaderBackgroundColor         ?? _Md3HeaderBgLight;
        _HeaderBgDark   = options?.HeaderBackgroundColorDark     ?? options?.HeaderBackgroundColor ?? _Md3HeaderBgDark;
        _HeaderText     = options?.HeaderTextColor               ?? Colors.White;
        _Surface        = options?.SurfaceColor                  ?? _Md3Surface;
        _SurfaceDark    = options?.SurfaceColorDark              ?? options?.SurfaceColor ?? _Md3SurfaceDark;
        _OnSurface      = options?.OnSurfaceColor                ?? _Md3OnSurface;
        _OnSurfaceDark  = options?.OnSurfaceColorDark            ?? options?.OnSurfaceColor ?? _Md3OnSurfaceDark;
        _OnSurfaceVar   = options?.OnSurfaceVariantColor         ?? _Md3OnSurfaceVar;
        _OnSurfaceVarDk = options?.OnSurfaceVariantColorDark     ?? options?.OnSurfaceVariantColor ?? _Md3OnSurfaceVarDk;
        _FontFamily       = options?.FontFamily;
        _UseNepaliScript  = options?.UseNepaliScript ?? false;

        _DisplayMode = options?.DisplayMode ?? DateDisplayMode.Both;
        _PickerStyle = options?.PickerStyle ?? PickerStyle.Calendar;
        _IsBsMode    = _DisplayMode != DateDisplayMode.AdOnly;

        var seed = initial ?? BsAdConverter.AdToBs(DateTime.Today);
        _BsYear = seed.Year; _BsMonth = seed.Month; _BsDay = seed.Day;
        _AdDate = BsAdConverter.BsToAd(seed);
        _ViewYear  = _IsBsMode ? _BsYear : _AdDate.Year;
        _ViewMonth = _IsBsMode ? _BsMonth : _AdDate.Month;
        _PickerSelectedYear = _ViewYear;

        // ── Drag handle (bottom-sheet only) ──────────────────────────────────
        bool isDialog = (options?.Presentation ?? PickerPresentation.BottomSheet) == PickerPresentation.Dialog;
        var handle = new BoxView
        {
            HeightRequest     = 4,
            WidthRequest      = 32,
            CornerRadius      = 2,
            HorizontalOptions = LayoutOptions.Center,
            Margin            = new Thickness(0, 10, 0, 0),
            IsVisible         = !isDialog,
        };
        handle.SetAppThemeColor(BoxView.ColorProperty,
            Color.FromArgb("#CCCCCC"), Color.FromArgb("#49454F"));

        // ── Header ────────────────────────────────────────────────────────────
        var selectLabel = new Label
        {
            Text = UseNepaliGlyphs ? "मिति छान्नुहोस्" : "SELECT DATE",
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            CharacterSpacing = UseNepaliGlyphs ? 0 : 1.5,
            TextColor = Color.FromRgba((byte)255, (byte)255, (byte)255, (byte)178),
        };
        ApplyFont(selectLabel, UseNepaliGlyphs);

        _HeaderDateLabel = new Label
        {
            FontSize = 22,
            FontAttributes = FontAttributes.Bold,
            TextColor = _HeaderText,
        };
        ApplyFont(_HeaderDateLabel, UseNepaliGlyphs);

        _ChevronLabel = new Label
        {
            Text = "▾",
            FontSize = 13,
            VerticalTextAlignment = TextAlignment.Center,
        };
        _ChevronLabel.SetAppThemeColor(Label.TextColorProperty, _OnSurface, _OnSurfaceDark);

        View headerDateRow;
        if (_PickerStyle == PickerStyle.Input)
        {
            var calIcon = new Label
            {
                Text = "🗓",
                FontSize = 20,
                TextColor = _HeaderText,
                VerticalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0),
            };
            var dateGrid = new Grid
            {
                Margin = new Thickness(0, 4, 0, 2),
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                },
            };
            dateGrid.Add(_HeaderDateLabel);
            Grid.SetColumn(calIcon, 1);
            dateGrid.Add(calIcon);
            headerDateRow = dateGrid;
        }
        else
        {
            headerDateRow = new HorizontalStackLayout
            {
                Spacing = 0,
                Margin = new Thickness(0, 4, 0, 2),
                HorizontalOptions = LayoutOptions.Start,
                Children = { _HeaderDateLabel },
            };
        }

        _HeaderEquivLabel = new Label
        {
            FontSize = 12,
            TextColor = Color.FromRgba((byte)255, (byte)255, (byte)255, (byte)178),
        };
        ApplyFont(_HeaderEquivLabel, false);

        var headerContent = new VerticalStackLayout
        {
            Padding = new Thickness(20, 14, 20, 14),
            Spacing = 0,
            Children = { selectLabel, headerDateRow, _HeaderEquivLabel },
        };
        headerContent.SetAppThemeColor(VisualElement.BackgroundColorProperty, _HeaderBg, _HeaderBgDark);

        // ── BS / AD chip toggle ───────────────────────────────────────────────
        _BsChip = BuildChip("BS");
        _AdChip = BuildChip("AD");

        var bsTap = new TapGestureRecognizer();
        bsTap.Tapped += (_, _) => SetMode(bs: true);
        _BsChip.GestureRecognizers.Add(bsTap);

        var adTap = new TapGestureRecognizer();
        adTap.Tapped += (_, _) => SetMode(bs: false);
        _AdChip.GestureRecognizers.Add(adTap);

        var chipRow = new HorizontalStackLayout
        {
            Spacing = 8,
            Padding = new Thickness(16, 10, 16, 2),
            IsVisible = _DisplayMode == DateDisplayMode.Both,
            Children = { _BsChip, _AdChip },
        };

        // ── Month / year navigation ───────────────────────────────────────────
        _PrevBtn = BuildNavButton("‹");
        _PrevBtn.Clicked += (_, _) => Navigate(-1);
        _NextBtn = BuildNavButton("›");
        _NextBtn.Clicked += (_, _) => Navigate(+1);

        _MonthYearLabel = new Label
        {
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            VerticalTextAlignment = TextAlignment.Center,
        };
        _MonthYearLabel.SetAppThemeColor(Label.TextColorProperty, _OnSurface, _OnSurfaceDark);
        ApplyFont(_MonthYearLabel, UseNepaliGlyphs);

        var monthYearRow = new HorizontalStackLayout
        {
            Spacing = 4,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions   = LayoutOptions.Center,
            Children = { _MonthYearLabel, _ChevronLabel },
        };
        var monthYearTap = new TapGestureRecognizer();
        monthYearTap.Tapped += (_, _) => ToggleYearMonthPicker();
        monthYearRow.GestureRecognizers.Add(monthYearTap);

        var navRow = new Grid { HeightRequest = 44, Padding = new Thickness(4, 0) };
        navRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        navRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        navRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        navRow.Add(_PrevBtn);
        Grid.SetColumn(monthYearRow, 1); navRow.Add(monthYearRow);
        Grid.SetColumn(_NextBtn, 2);     navRow.Add(_NextBtn);

        // ── Day-of-week header ────────────────────────────────────────────────
        _DowRow = BuildDowRow();

        // ── Calendar grid host ────────────────────────────────────────────────
        _CalendarHost = new ContentView { Padding = new Thickness(12, 4, 12, 8) };

        // ── Divider ───────────────────────────────────────────────────────────
        var divider = new BoxView { HeightRequest = 1 };
        divider.SetAppThemeColor(BoxView.ColorProperty,
            Color.FromArgb("#E7E0EC"), Color.FromArgb("#49454F"));

        // ── Action buttons ────────────────────────────────────────────────────
        var cancelBtn = new Button
        {
            Text = "CANCEL",
            BackgroundColor = Colors.Transparent,
            BorderWidth = 0,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            Padding = new Thickness(16, 0),
            HeightRequest = 40,
        };
        cancelBtn.SetAppThemeColor(Button.TextColorProperty, _Primary, _PrimaryDark);
        if (_FontFamily is not null) cancelBtn.FontFamily = _FontFamily;
        cancelBtn.Clicked += (_, _) => Cancelled?.Invoke(this, EventArgs.Empty);

        _OkBtn = new Button
        {
            Text = "OK",
            BackgroundColor = Colors.Transparent,
            BorderWidth = 0,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            Padding = new Thickness(16, 0),
            HeightRequest = 40,
        };
        _OkBtn.SetAppThemeColor(Button.TextColorProperty, _Primary, _PrimaryDark);
        if (_FontFamily is not null) _OkBtn.FontFamily = _FontFamily;
        _OkBtn.Clicked += (_, _) => CommitAndClose();

        var actionRow = new HorizontalStackLayout
        {
            HorizontalOptions = LayoutOptions.End,
            Padding = new Thickness(0, 4, 8, 12),
            Spacing = 0,
            Children = { cancelBtn, _OkBtn },
        };

        // ── Root ──────────────────────────────────────────────────────────────
        var root = new VerticalStackLayout { Spacing = 0 };
        root.SetAppThemeColor(VisualElement.BackgroundColorProperty, _Surface, _SurfaceDark);
        root.Children.Add(handle);
        root.Children.Add(headerContent);
        root.Children.Add(chipRow);
        if (_PickerStyle == PickerStyle.Calendar)
        {
            root.Children.Add(navRow);
            root.Children.Add(_DowRow);
        }
        root.Children.Add(_CalendarHost);
        root.Children.Add(divider);
        root.Children.Add(actionRow);

        Content = root;

        ApplyChipState();
        RefreshHeader();
        if (_PickerStyle == PickerStyle.Wheel)
        {
            BuildWheels();
            RebuildWheelItems();
        }
        else if (_PickerStyle == PickerStyle.Input)
        {
            BuildInputField();
        }
        else
        {
            RefreshMonthYear();
            RebuildCalendar();
        }
    }

    // ── BS / AD mode switching ────────────────────────────────────────────────

    private void SetMode(bool bs)
    {
        if (_DisplayMode != DateDisplayMode.Both) return;
        if (_IsBsMode == bs) return;
        _IsBsMode = bs;

        if (bs)
        {
            var synced = BsAdConverter.AdToBs(_AdDate);
            _BsYear = synced.Year; _BsMonth = synced.Month; _BsDay = synced.Day;
            _ViewYear = _BsYear; _ViewMonth = _BsMonth;
        }
        else
        {
            _AdDate = BsAdConverter.BsToAd(new NepaliDate(_BsYear, _BsMonth, _BsDay));
            _ViewYear = _AdDate.Year; _ViewMonth = _AdDate.Month;
        }

        // Exit year/month picker when switching calendar system
        if (_PickerMode == PickerMode.YearMonth)
        {
            _PickerMode = PickerMode.Calendar;
            SyncModeUI();
        }

        _PickerSelectedYear = _ViewYear;
        ApplyChipState();
        RefreshHeader();
        if (_PickerStyle == PickerStyle.Wheel)
        {
            RebuildWheelItems();
        }
        else if (_PickerStyle == PickerStyle.Input)
        {
            RefreshInputEntry();
        }
        else
        {
            RefreshMonthYear();
            RefreshDowRow();
            RebuildCalendar();
        }
    }

    private void ApplyChipState()
    {
        _BsChip.BackgroundColor = _IsBsMode ? _Primary : Colors.Transparent;
        _AdChip.BackgroundColor = _IsBsMode ? Colors.Transparent : _Primary;

        _BsChip.Stroke = _IsBsMode ? Colors.Transparent : new SolidColorBrush(_Primary);
        _AdChip.Stroke = _IsBsMode ? new SolidColorBrush(_Primary) : Colors.Transparent;

        if (_BsChip.Content is Label bsLbl) bsLbl.TextColor = _IsBsMode ? _OnPrimary : _Primary;
        if (_AdChip.Content is Label adLbl) adLbl.TextColor = _IsBsMode ? _Primary   : _OnPrimary;
    }

    // ── Year/month picker toggle ──────────────────────────────────────────────

    private void ToggleYearMonthPicker()
    {
        _PickerMode = _PickerMode == PickerMode.Calendar
            ? PickerMode.YearMonth
            : PickerMode.Calendar;

        if (_PickerMode == PickerMode.YearMonth)
            _PickerSelectedYear = _ViewYear;

        SyncModeUI();
        RebuildCalendar();
    }

    private void SyncModeUI()
    {
        bool isCalendar    = _PickerMode == PickerMode.Calendar;
        _ChevronLabel.Text = isCalendar ? "▾" : "▴";
        _PrevBtn.IsVisible = isCalendar;
        _NextBtn.IsVisible = isCalendar;
        _DowRow.IsVisible  = isCalendar;
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    private void Navigate(int delta)
    {
        _ViewMonth += delta;
        if (_ViewMonth < 1)       { _ViewMonth = 12; _ViewYear--; }
        else if (_ViewMonth > 12) { _ViewMonth = 1;  _ViewYear++; }

        _ViewYear = _IsBsMode
            ? Math.Clamp(_ViewYear, BsCalendarData.MinYear, BsCalendarData.MaxYear)
            : Math.Clamp(_ViewYear, 1900, 2100);

        RefreshMonthYear();
        RebuildCalendar();
    }

    // ── Day tapped ────────────────────────────────────────────────────────────

    private void OnDayTapped(int day)
    {
        if (_IsBsMode)
        {
            _BsYear = _ViewYear; _BsMonth = _ViewMonth; _BsDay = day;
            _AdDate = BsAdConverter.BsToAd(new NepaliDate(_BsYear, _BsMonth, _BsDay));
        }
        else
        {
            _AdDate = new DateTime(_ViewYear, _ViewMonth, day);
            var bs = BsAdConverter.AdToBs(_AdDate);
            _BsYear = bs.Year; _BsMonth = bs.Month; _BsDay = bs.Day;
        }

        RefreshHeader();
        RebuildCalendar();
    }

    // ── Year/month picker handlers ────────────────────────────────────────────

    private void OnYearTapped(int year)
    {
        _PickerSelectedYear = year;
        RebuildCalendar(); // re-render year grid with new highlight; scroll preserved via Loaded
    }

    private void OnMonthTapped(int month)
    {
        _ViewYear  = _PickerSelectedYear;
        _ViewMonth = month;

        // Clamp current selection into the new year+month
        if (_IsBsMode)
        {
            int maxDay = BsCalendarData.GetDaysInMonth(_ViewYear, _ViewMonth);
            _BsDay   = Math.Min(_BsDay, maxDay);
            _BsYear  = _ViewYear;
            _BsMonth = _ViewMonth;
            _AdDate  = BsAdConverter.BsToAd(new NepaliDate(_BsYear, _BsMonth, _BsDay));
        }
        else
        {
            int maxDay = DateTime.DaysInMonth(_ViewYear, _ViewMonth);
            int day    = Math.Min(_AdDate.Day, maxDay);
            _AdDate    = new DateTime(_ViewYear, _ViewMonth, day);
            var bs     = BsAdConverter.AdToBs(_AdDate);
            _BsYear = bs.Year; _BsMonth = bs.Month; _BsDay = bs.Day;
        }

        _PickerMode = PickerMode.Calendar;
        SyncModeUI();
        RefreshHeader();
        RefreshMonthYear();
        RebuildCalendar();
    }

    private void CommitAndClose()
        => Done?.Invoke(this, new NepaliDate(_BsYear, _BsMonth, _BsDay));

    // ── Header & month label ──────────────────────────────────────────────────

    private void RefreshHeader()
    {
        ApplyFont(_HeaderDateLabel, UseNepaliGlyphs);
        DateTime ad = BsAdConverter.BsToAd(new NepaliDate(_BsYear, _BsMonth, _BsDay));

        if (_IsBsMode)
        {
            if (_UseNepaliScript)
            {
                var dow = _DowHeaderNepali[(int)ad.DayOfWeek];
                _HeaderDateLabel.Text = $"{dow}, {NepaliDate.MonthNamesNepali[_BsMonth - 1]} {N(_BsDay)}, {N(_BsYear)}";
            }
            else
            {
                _HeaderDateLabel.Text = $"{ad:ddd}, {NepaliDate.MonthNames[_BsMonth - 1]} {_BsDay}, {_BsYear}";
            }
            _HeaderEquivLabel.Text = _DisplayMode == DateDisplayMode.BsOnly
                ? string.Empty
                : $"AD  {ad:d MMMM yyyy}";
        }
        else
        {
            if (UseNepaliGlyphs)
            {
                var dow = _DowHeaderNepali[(int)_AdDate.DayOfWeek];
                _HeaderDateLabel.Text = $"{dow}, {NepaliDate.AdMonthNamesNepali[_AdDate.Month - 1]} {N(_AdDate.Day)}, {N(_AdDate.Year)}";
            }
            else
            {
                _HeaderDateLabel.Text = $"{_AdDate:ddd, MMMM d, yyyy}";
            }
            _HeaderEquivLabel.Text = _DisplayMode == DateDisplayMode.AdOnly
                ? string.Empty
                : $"BS  {new NepaliDate(_BsYear, _BsMonth, _BsDay).ToDisplayString()}";
        }
    }

    private void RefreshMonthYear()
    {
        ApplyFont(_MonthYearLabel, UseNepaliGlyphs);
        if (_IsBsMode)
        {
            var month = _UseNepaliScript ? NepaliDate.MonthNamesNepali[_ViewMonth - 1] : NepaliDate.MonthNames[_ViewMonth - 1];
            var year  = _UseNepaliScript ? N(_ViewYear) : _ViewYear.ToString();
            _MonthYearLabel.Text = $"{month}  {year}";
        }
        else
        {
            _MonthYearLabel.Text = UseNepaliGlyphs
                ? $"{NepaliDate.AdMonthNamesNepali[_ViewMonth - 1]}  {N(_ViewYear)}"
                : $"{new DateTime(_ViewYear, _ViewMonth, 1):MMMM yyyy}";
        }
    }

    // ── Calendar / year-month dispatch ────────────────────────────────────────

    private void RebuildCalendar()
    {
        if (_PickerMode == PickerMode.YearMonth)
        {
            _CalendarHost.Content       = BuildYearMonthPickerView();
            _CalendarHost.HeightRequest = YearMonthPickerHeight();
        }
        else
        {
            var (grid, _) = BuildCalendarGrid();
            _CalendarHost.Content       = grid;
            _CalendarHost.HeightRequest = 6 * 44 + 5 * 2 + 12; // fixed 6-row height = 286
        }
    }

    private static double YearMonthPickerHeight()
    {
        // year scroll + separator (1 + 6+6 margin) + month grid + calendarHost padding (4+8)
        double yearScroll  = _YearVisible * (_YearCellH + _YearSpacing) - _YearSpacing;  // 150
        const double sep   = 1 + 6 + 6;                                               //  13
        double monthGrid   = 4 * _MonthCellH + 3 * _MonthSpacing;                       // 182
        return yearScroll + sep + monthGrid + 12;                                      // 357
    }

    // ── Year + month picker view ──────────────────────────────────────────────

    private View BuildYearMonthPickerView()
    {
        var root = new VerticalStackLayout { Spacing = 0 };

        // ── Year grid (scrollable) ────────────────────────────────────────────
        int minYear   = _IsBsMode ? BsCalendarData.MinYear : 1900;
        int maxYear   = _IsBsMode ? BsCalendarData.MaxYear : 2100;
        int yearCount = maxYear - minYear + 1;
        int yearRows  = (int)Math.Ceiling(yearCount / (double)_YearCols);
        int todayYear = _IsBsMode
            ? BsAdConverter.AdToBs(DateTime.Today).Year
            : DateTime.Today.Year;

        var yearGrid = new Grid { RowSpacing = _YearSpacing, ColumnSpacing = _YearSpacing };
        for (int c = 0; c < _YearCols; c++)
            yearGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        for (int r = 0; r < yearRows; r++)
            yearGrid.RowDefinitions.Add(new RowDefinition(new GridLength(_YearCellH)));

        for (int i = 0; i < yearCount; i++)
        {
            int year = minYear + i;
            var cell = BuildYearCell(year, year == _PickerSelectedYear, year == todayYear);
            Grid.SetRow(cell, i / _YearCols);
            Grid.SetColumn(cell, i % _YearCols);

            int captured = year;
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => OnYearTapped(captured);
            cell.GestureRecognizers.Add(tap);
            yearGrid.Add(cell);
        }

        double yearScrollH = _YearVisible * (_YearCellH + _YearSpacing) - _YearSpacing;
        int    selRow      = (_PickerSelectedYear - minYear) / _YearCols;
        double scrollY     = Math.Max(0, (selRow - 1) * (_YearCellH + _YearSpacing));

        var yearScroll = new ScrollView
        {
            Content       = yearGrid,
            HeightRequest = yearScrollH,
            Opacity       = 0,  // hidden until scrolled to avoid visible jump from top
        };

        // Loaded fires before Android's native layout pass on Android, so ScrollToAsync
        // is a no-op at that point. Task.Delay yields past those passes before scrolling,
        // and Opacity stays 0 until scroll is done so the user never sees the jump.
        yearScroll.Loaded += async (_, _) =>
        {
            await Task.Delay(50);
            await yearScroll.ScrollToAsync(0, scrollY, false);
            yearScroll.Opacity = 1;
        };

        root.Children.Add(yearScroll);

        // ── Separator ─────────────────────────────────────────────────────────
        var sep = new BoxView { HeightRequest = 1, Margin = new Thickness(0, 6) };
        sep.SetAppThemeColor(BoxView.ColorProperty,
            Color.FromArgb("#E7E0EC"), Color.FromArgb("#49454F"));
        root.Children.Add(sep);

        // ── Month grid (3 × 4) ────────────────────────────────────────────────
        string[] monthNames = _IsBsMode
            ? (_UseNepaliScript ? NepaliDate.MonthNamesNepali : NepaliDate.MonthNames)
            : (UseNepaliGlyphs ? NepaliDate.AdMonthNamesNepali : _AdMonthNames);
        bool monthIsDevanagari = _IsBsMode ? _UseNepaliScript : UseNepaliGlyphs;

        var monthGrid = new Grid { RowSpacing = _MonthSpacing, ColumnSpacing = _MonthSpacing };
        for (int c = 0; c < _MonthCols; c++)
            monthGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        for (int r = 0; r < 4; r++)
            monthGrid.RowDefinitions.Add(new RowDefinition(new GridLength(_MonthCellH)));

        for (int i = 0; i < 12; i++)
        {
            int  month     = i + 1;
            bool isCurrent = month == _ViewMonth && _PickerSelectedYear == _ViewYear;
            var  cell      = BuildMonthCell(monthNames[i], isCurrent, monthIsDevanagari);
            Grid.SetRow(cell, i / _MonthCols);
            Grid.SetColumn(cell, i % _MonthCols);

            int captured = month;
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => OnMonthTapped(captured);
            cell.GestureRecognizers.Add(tap);
            monthGrid.Add(cell);
        }

        root.Children.Add(monthGrid);
        return root;
    }

    // ── Year cell ─────────────────────────────────────────────────────────────

    private View BuildYearCell(int year, bool isSelected, bool isCurrentYear)
    {
        var label = new Label
        {
            Text = UseNepaliGlyphs ? N(year) : year.ToString(),
            FontSize = 13,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment   = TextAlignment.Center,
            HorizontalOptions       = LayoutOptions.Fill,
            VerticalOptions         = LayoutOptions.Fill,
        };
        ApplyFont(label, UseNepaliGlyphs);

        if (isSelected)
        {
            label.TextColor      = _OnPrimary;
            label.FontAttributes = FontAttributes.Bold;
            return new Border
            {
                BackgroundColor   = _Primary,
                Stroke            = Colors.Transparent,
                StrokeThickness   = 0,
                StrokeShape       = new RoundRectangle { CornerRadius = 8 },
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions   = LayoutOptions.Fill,
                Content           = label,
            };
        }

        if (isCurrentYear)
        {
            label.TextColor      = _Primary;
            label.FontAttributes = FontAttributes.Bold;
            return new Border
            {
                BackgroundColor   = Colors.Transparent,
                Stroke            = new SolidColorBrush(_Primary),
                StrokeThickness   = 1,
                StrokeShape       = new RoundRectangle { CornerRadius = 8 },
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions   = LayoutOptions.Fill,
                Content           = label,
            };
        }

        label.SetAppThemeColor(Label.TextColorProperty, _OnSurface, _OnSurfaceDark);
        return label;
    }

    // ── Month cell ────────────────────────────────────────────────────────────

    private View BuildMonthCell(string name, bool isSelected, bool devanagari = false)
    {
        var label = new Label
        {
            Text = name,
            FontSize = 13,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment   = TextAlignment.Center,
            HorizontalOptions       = LayoutOptions.Fill,
            VerticalOptions         = LayoutOptions.Fill,
        };
        ApplyFont(label, devanagari);

        if (isSelected)
        {
            label.TextColor      = _OnPrimary;
            label.FontAttributes = FontAttributes.Bold;
            return new Border
            {
                BackgroundColor   = _Primary,
                Stroke            = Colors.Transparent,
                StrokeThickness   = 0,
                StrokeShape       = new RoundRectangle { CornerRadius = 8 },
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions   = LayoutOptions.Fill,
                Content           = label,
            };
        }

        label.SetAppThemeColor(Label.TextColorProperty, _OnSurface, _OnSurfaceDark);
        return label;
    }

    // ── Calendar grid ─────────────────────────────────────────────────────────

    private (Grid grid, int rowCount) BuildCalendarGrid()
    {
        int daysInMonth, startDow, todayDay = -1, selectedDay = -1;

        if (_IsBsMode)
        {
            daysInMonth = BsCalendarData.GetDaysInMonth(_ViewYear, _ViewMonth);
            startDow    = (int)BsAdConverter.BsToAd(new NepaliDate(_ViewYear, _ViewMonth, 1)).DayOfWeek;

            var todayBs = BsAdConverter.AdToBs(DateTime.Today);
            if (todayBs.Year == _ViewYear && todayBs.Month == _ViewMonth)
                todayDay = todayBs.Day;

            if (_BsYear == _ViewYear && _BsMonth == _ViewMonth)
                selectedDay = _BsDay;
        }
        else
        {
            daysInMonth = DateTime.DaysInMonth(_ViewYear, _ViewMonth);
            startDow    = (int)new DateTime(_ViewYear, _ViewMonth, 1).DayOfWeek;

            if (DateTime.Today.Year == _ViewYear && DateTime.Today.Month == _ViewMonth)
                todayDay = DateTime.Today.Day;

            if (_AdDate.Year == _ViewYear && _AdDate.Month == _ViewMonth)
                selectedDay = _AdDate.Day;
        }

        int rowCount = (int)Math.Ceiling((startDow + daysInMonth) / 7.0);

        var grid = new Grid { RowSpacing = 2, ColumnSpacing = 0 };
        for (int c = 0; c < 7; c++)
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        for (int r = 0; r < rowCount; r++)
            grid.RowDefinitions.Add(new RowDefinition(new GridLength(44)));

        int dayNum = 1;
        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < 7; col++)
            {
                if (row * 7 + col < startDow) continue;
                if (dayNum > daysInMonth) break;

                bool isSelected = dayNum == selectedDay;
                bool isToday    = dayNum == todayDay;

                var cell = BuildDayCell(dayNum, isSelected, isToday);
                Grid.SetRow(cell, row);
                Grid.SetColumn(cell, col);

                int captured = dayNum;
                var tap = new TapGestureRecognizer();
                tap.Tapped += (_, _) => OnDayTapped(captured);
                cell.GestureRecognizers.Add(tap);

                grid.Add(cell);
                dayNum++;
            }
        }

        return (grid, rowCount);
    }

    // ── Wheel style (iOS drum roll) ───────────────────────────────────────────

    private void BuildWheels()
    {
        string? wheelFont = UseNepaliGlyphs ? _FontFamily : null;
        _YearWheel  = new DrumRollPicker { FontFamily = wheelFont };
        _MonthWheel = new DrumRollPicker { FontFamily = wheelFont };
        _DayWheel   = new DrumRollPicker { FontFamily = wheelFont };

        _YearWheel.SelectionChanged  += (_, _) => OnWheelChanged();
        _MonthWheel.SelectionChanged += (_, _) => OnWheelChanged();
        _DayWheel.SelectionChanged   += (_, _) => OnWheelChanged();

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
            },
        };
        grid.Add(_YearWheel);
        Grid.SetColumn(_MonthWheel, 1); grid.Add(_MonthWheel);
        Grid.SetColumn(_DayWheel, 2);   grid.Add(_DayWheel);

        _CalendarHost.Content       = grid;
        _CalendarHost.HeightRequest = 5 * 44 + 12;  // wheel height + host padding
    }

    /// <summary>Rebuilds all wheel items for the current calendar system and script,
    /// then positions the wheels on the current selection.</summary>
    private void RebuildWheelItems()
    {
        if (_YearWheel is null || _MonthWheel is null || _DayWheel is null) return;

        int minYear = _IsBsMode ? BsCalendarData.MinYear : 1900;
        int maxYear = _IsBsMode ? BsCalendarData.MaxYear : 2100;
        _WheelMinYear = minYear;

        var years = new string[maxYear - minYear + 1];
        for (int y = minYear; y <= maxYear; y++)
            years[y - minYear] = UseNepaliGlyphs ? N(y) : y.ToString();

        _WheelSyncing = true;

        string? wheelFont = UseNepaliGlyphs ? _FontFamily : null;
        _YearWheel.FontFamily  = wheelFont;
        _MonthWheel.FontFamily = wheelFont;
        _DayWheel.FontFamily   = wheelFont;

        _YearWheel.Items  = years;
        _MonthWheel.Items = _IsBsMode
            ? (_UseNepaliScript ? NepaliDate.MonthNamesNepali : NepaliDate.MonthNames)
            : (UseNepaliGlyphs ? NepaliDate.AdMonthNamesNepali : _AdMonthNames);
        _WheelDayCount = 0;  // force the day wheel to re-render in the current script

        int year  = _IsBsMode ? _BsYear  : _AdDate.Year;
        int month = _IsBsMode ? _BsMonth : _AdDate.Month;
        int day   = _IsBsMode ? _BsDay   : _AdDate.Day;

        _YearWheel.SelectedIndex  = Math.Clamp(year - minYear, 0, years.Length - 1);
        _MonthWheel.SelectedIndex = month - 1;
        RefreshWheelDays(minYear + _YearWheel.SelectedIndex, month);
        _DayWheel.SelectedIndex   = Math.Clamp(day - 1, 0, _WheelDayCount - 1);

        _WheelSyncing = false;
    }

    /// <summary>Resizes the day wheel to the given month, keeping the day selection clamped.</summary>
    private void RefreshWheelDays(int year, int month)
    {
        if (_DayWheel is null) return;

        int days = _IsBsMode
            ? BsCalendarData.GetDaysInMonth(year, month)
            : DateTime.DaysInMonth(year, month);
        if (days == _WheelDayCount) return;

        if (_DayWheel.SelectedIndex > days - 1)
            _DayWheel.SelectedIndex = days - 1;

        var items = new string[days];
        for (int d = 1; d <= days; d++)
            items[d - 1] = UseNepaliGlyphs ? N(d) : d.ToString();

        _WheelDayCount  = days;
        _DayWheel.Items = items;
    }

    private void OnWheelChanged()
    {
        if (_WheelSyncing || _YearWheel is null || _MonthWheel is null || _DayWheel is null) return;

        int year  = _WheelMinYear + _YearWheel.SelectedIndex;
        int month = _MonthWheel.SelectedIndex + 1;

        _WheelSyncing = true;
        RefreshWheelDays(year, month);
        _WheelSyncing = false;

        int day = _DayWheel.SelectedIndex + 1;

        if (_IsBsMode)
        {
            _BsYear = year; _BsMonth = month; _BsDay = day;
            _AdDate = BsAdConverter.BsToAd(new NepaliDate(year, month, day));
        }
        else
        {
            _AdDate = new DateTime(year, month, day);
            var bs  = BsAdConverter.AdToBs(_AdDate);
            _BsYear = bs.Year; _BsMonth = bs.Month; _BsDay = bs.Day;
        }

        RefreshHeader();
    }

    // ── Input style (desktop text entry) ─────────────────────────────────────

    private void BuildInputField()
    {
        string initialText = _IsBsMode
            ? $"{_BsYear:D4}-{_BsMonth:D2}-{_BsDay:D2}"
            : $"{_AdDate.Year:D4}-{_AdDate.Month:D2}-{_AdDate.Day:D2}";

        _InputEntry = new Entry
        {
            Text = initialText,
            Placeholder = "YYYY-MM-DD",
            Keyboard = Keyboard.Default,
            MaxLength = 10,
            FontSize = 16,
            BackgroundColor = Colors.Transparent,
        };
        if (_FontFamily is not null) _InputEntry.FontFamily = _FontFamily;
        _InputEntry.SetAppThemeColor(Entry.TextColorProperty, _OnSurface, _OnSurfaceDark);
        _InputEntry.TextChanged += (_, e) => ValidateAndApplyInput(e.NewTextValue);

        var fieldLabel = new Label { Text = "Enter Date", FontSize = 11 };
        fieldLabel.SetAppThemeColor(Label.TextColorProperty, _OnSurfaceVar, _OnSurfaceVarDk);
        ApplyFont(fieldLabel, false);

        var borderColor = Application.Current?.RequestedTheme == AppTheme.Dark ? _PrimaryDark : _Primary;
        var entryBorder = new Border
        {
            Stroke = new SolidColorBrush(borderColor),
            StrokeThickness = 1.5,
            StrokeShape = new RoundRectangle { CornerRadius = 4 },
            Padding = new Thickness(12, 8),
            Content = new VerticalStackLayout
            {
                Spacing = 4,
                Children = { fieldLabel, _InputEntry },
            },
        };

        _CalendarHost.Content = entryBorder;
    }

    private void ValidateAndApplyInput(string text)
    {
        if (_OkBtn is null) return;

        if (text.Length != 10 || text[4] != '-' || text[7] != '-')
        {
            _OkBtn.IsEnabled = false;
            return;
        }

        if (!int.TryParse(text.AsSpan(0, 4), out int year) ||
            !int.TryParse(text.AsSpan(5, 2), out int month) ||
            !int.TryParse(text.AsSpan(8, 2), out int day))
        {
            _OkBtn.IsEnabled = false;
            return;
        }

        if (_IsBsMode)
        {
            if (year < BsCalendarData.MinYear || year > BsCalendarData.MaxYear ||
                month < 1 || month > 12)
            {
                _OkBtn.IsEnabled = false;
                return;
            }
            int maxDay = BsCalendarData.GetDaysInMonth(year, month);
            if (day < 1 || day > maxDay)
            {
                _OkBtn.IsEnabled = false;
                return;
            }
            _BsYear = year; _BsMonth = month; _BsDay = day;
            _AdDate = BsAdConverter.BsToAd(new NepaliDate(year, month, day));
        }
        else
        {
            if (year < 1900 || year > 2100 || month < 1 || month > 12)
            {
                _OkBtn.IsEnabled = false;
                return;
            }
            int maxDay = DateTime.DaysInMonth(year, month);
            if (day < 1 || day > maxDay)
            {
                _OkBtn.IsEnabled = false;
                return;
            }
            _AdDate = new DateTime(year, month, day);
            var bs = BsAdConverter.AdToBs(_AdDate);
            _BsYear = bs.Year; _BsMonth = bs.Month; _BsDay = bs.Day;
        }

        _OkBtn.IsEnabled = true;
        RefreshHeader();
    }

    private void RefreshInputEntry()
    {
        if (_InputEntry is null) return;
        _InputEntry.Text = _IsBsMode
            ? $"{_BsYear:D4}-{_BsMonth:D2}-{_BsDay:D2}"
            : $"{_AdDate.Year:D4}-{_AdDate.Month:D2}-{_AdDate.Day:D2}";
    }

    // ── Day cell ──────────────────────────────────────────────────────────────

    private View BuildDayCell(int day, bool isSelected, bool isToday)
    {
        var label = new Label
        {
            Text = UseNepaliGlyphs ? N(day) : day.ToString(),
            FontSize = 14,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment   = TextAlignment.Center,
            HorizontalOptions       = LayoutOptions.Fill,
            VerticalOptions         = LayoutOptions.Fill,
        };
        ApplyFont(label, UseNepaliGlyphs);

        if (isSelected)
        {
            label.TextColor      = _OnPrimary;
            label.FontAttributes = FontAttributes.Bold;
            return new Border
            {
                BackgroundColor   = _Primary,
                Stroke            = Colors.Transparent,
                StrokeThickness   = 0,
                StrokeShape       = new RoundRectangle { CornerRadius = 20 },
                WidthRequest      = 40,
                HeightRequest     = 40,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions   = LayoutOptions.Center,
                Content           = label,
            };
        }

        if (isToday)
        {
            label.TextColor      = _Primary;
            label.FontAttributes = FontAttributes.Bold;
            return new Border
            {
                BackgroundColor   = Colors.Transparent,
                Stroke            = new SolidColorBrush(_Primary),
                StrokeThickness   = 1.5,
                StrokeShape       = new RoundRectangle { CornerRadius = 20 },
                WidthRequest      = 40,
                HeightRequest     = 40,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions   = LayoutOptions.Center,
                Content           = label,
            };
        }

        label.SetAppThemeColor(Label.TextColorProperty, _OnSurface, _OnSurfaceDark);
        return label;
    }

    // ── Day-of-week header row ────────────────────────────────────────────────

    private Grid BuildDowRow()
    {
        var grid = new Grid { HeightRequest = 36, Padding = new Thickness(12, 0) };
        for (int i = 0; i < 7; i++)
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        var labels = UseNepaliGlyphs ? _DowLabelsNepali : _DowLabels;
        for (int i = 0; i < 7; i++)
        {
            var lbl = new Label
            {
                Text = labels[i],
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment   = TextAlignment.Center,
            };
            lbl.SetAppThemeColor(Label.TextColorProperty, _OnSurfaceVar, _OnSurfaceVarDk);
            ApplyFont(lbl, UseNepaliGlyphs);
            Grid.SetColumn(lbl, i);
            grid.Add(lbl);
        }
        return grid;
    }

    private void RefreshDowRow()
    {
        var labels = UseNepaliGlyphs ? _DowLabelsNepali : _DowLabels;
        int i = 0;
        foreach (var child in _DowRow.Children)
        {
            if (child is Label lbl) lbl.Text = labels[i++];
        }
    }

    // ── Builder helpers ───────────────────────────────────────────────────────

    private Border BuildChip(string text)
    {
        var label = new Label
        {
            Text = text,
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment   = TextAlignment.Center,
            Margin = new Thickness(16, 6),
        };
        ApplyFont(label, false);
        return new Border
        {
            StrokeShape         = new RoundRectangle { CornerRadius = 16 },
            StrokeThickness     = 1.5,
            Padding             = 0,
            MinimumWidthRequest = 64,
            Content             = label,
        };
    }

    private Button BuildNavButton(string glyph)
    {
        var btn = new Button
        {
            Text            = glyph,
            FontSize        = 22,
            BackgroundColor = Colors.Transparent,
            BorderWidth     = 0,
            Padding         = new Thickness(8, 0),
            WidthRequest    = 44,
            HeightRequest   = 44,
        };
        btn.SetAppThemeColor(Button.TextColorProperty, _OnSurface, _OnSurfaceDark);
        if (_FontFamily is not null) btn.FontFamily = _FontFamily;
        return btn;
    }

    // Always set FontFamily (even null) so the local setter wins over any app-level
    // implicit style (e.g. "OpenSansRegular") that would otherwise block Devanagari glyphs.
    // A null value makes MAUI fall back to the system font (NotoSans on Android), which
    // covers Devanagari natively and renders Latin correctly.
    // For Latin labels, force null regardless of the user's custom font: a Devanagari-only
    // font applied to Latin content may lack certain glyphs or have OpenType rules that
    // produce unexpected output for Latin text.
    private void ApplyFont(Label label, bool devanagari = false)
    {
        label.FontFamily = devanagari ? _FontFamily : null;
    }

    // Converts an integer to Nepali (Devanagari) numeral string.
    // pad=true zero-pads to 2 digits before converting (e.g. 5 → "०५").
    private static string N(int n, bool pad = false)
    {
        var s = pad ? n.ToString("D2") : n.ToString();
        return s
            .Replace('0', '०').Replace('1', '१').Replace('2', '२')
            .Replace('3', '३').Replace('4', '४').Replace('5', '५')
            .Replace('6', '६').Replace('7', '७').Replace('8', '८')
            .Replace('9', '९');
    }
}

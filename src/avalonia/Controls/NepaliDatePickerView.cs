using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using NepaliDatePicker.Data;
using NepaliDatePicker.Models;
using NepaliDatePicker.Services;

namespace NepaliDatePicker.Controls;

/// <summary>
/// Avalonia calendar date-picker panel.
/// Layout: colored header → BS/AD chip toggle → month navigation →
///         day-of-week headers → calendar grid → CANCEL / OK actions.
/// Tapping the month/year label toggles a year + month grid for fast navigation.
/// </summary>
internal class NepaliDatePickerView : UserControl
{
    private enum PickerMode { Calendar, YearMonth }

    // ── MD3 static defaults ───────────────────────────────────────────────────
    private static readonly Color _Md3Primary        = Color.Parse("#6750A4");
    private static readonly Color _Md3PrimaryDark    = Color.Parse("#D0BCFF");
    private static readonly Color _Md3OnPrimary      = Colors.White;
    private static readonly Color _Md3OnPrimaryDark  = Color.Parse("#381E72");
    private static readonly Color _Md3Surface        = Color.Parse("#FFFBFE");
    private static readonly Color _Md3SurfaceDark    = Color.Parse("#1C1B1F");
    private static readonly Color _Md3OnSurface      = Color.Parse("#1C1B1F");
    private static readonly Color _Md3OnSurfaceDark  = Color.Parse("#E6E1E5");
    private static readonly Color _Md3OnSurfaceVar   = Color.Parse("#49454F");
    private static readonly Color _Md3OnSurfaceVarDk = Color.Parse("#CAC4D0");
    private static readonly Color _Md3HeaderBgLight  = Color.Parse("#6750A4");
    private static readonly Color _Md3HeaderBgDark   = Color.Parse("#4A4458");

    // ── Effective colors ──────────────────────────────────────────────────────
    private readonly Color _Primary, _OnPrimary;
    private readonly Color _HeaderBg, _HeaderText;
    private readonly Color _Surface, _OnSurface, _OnSurfaceVar;
    private readonly string? _FontFamily;
    private readonly bool _UseNepaliScript;
    private readonly bool _IsDark;

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
    private readonly TextBlock _HeaderDateLabel;
    private readonly TextBlock _HeaderEquivLabel;
    private readonly TextBlock _MonthYearLabel;
    private readonly TextBlock _ChevronLabel;
    private readonly Button _PrevBtn;
    private readonly Button _NextBtn;
    private readonly Grid _DowRow;
    private readonly ContentControl _CalendarHost;
    private readonly Border _BsChip, _AdChip;

    // ── Input style state ─────────────────────────────────────────────────────
    private TextBox? _InputEntry;
    private Button? _OkBtn;
    private bool _FormattingInput;
    private string _LastInputText = "";

    private static readonly string[] _DowLabels       = ["S",   "M",   "T",     "W",   "T",     "F",     "S"   ];
    private static readonly string[] _DowLabelsNepali = ["आइ", "सो", "मं",    "बु", "बि",   "शु",   "श"   ];
    private static readonly string[] _DowHeaderNepali = ["आइत","सोम","मंगल",  "बुध","बिही", "शुक्र","शनि"];
    private static readonly string[] _AdMonthNames =
    [
        "January","February","March","April","May","June",
        "July","August","September","October","November","December"
    ];

    private const int _YearCellH    = 36;
    private const int _YearSpacing  = 2;
    private const int _YearCols     = 3;
    private const int _YearVisible  = 4;
    private const int _MonthCellH   = 44;
    private const int _MonthSpacing = 2;
    private const int _MonthCols    = 3;

    private bool UseNepaliGlyphs => _UseNepaliScript && (_IsBsMode || _DisplayMode == DateDisplayMode.AdOnly);

    public NepaliDatePickerView(NepaliDate? initial = null, NepaliDatePickerOptions? options = null)
    {
        _IsDark = Application.Current?.ActualThemeVariant == ThemeVariant.Dark;

        _Primary      = _IsDark ? (options?.PrimaryColorDark ?? options?.PrimaryColor ?? _Md3PrimaryDark)
                                : (options?.PrimaryColor ?? _Md3Primary);
        _OnPrimary    = _IsDark ? (options?.OnPrimaryColor ?? _Md3OnPrimaryDark)
                                : (options?.OnPrimaryColor ?? _Md3OnPrimary);
        _HeaderBg     = _IsDark ? (options?.HeaderBackgroundColorDark ?? options?.HeaderBackgroundColor ?? _Md3HeaderBgDark)
                                : (options?.HeaderBackgroundColor ?? _Md3HeaderBgLight);
        _HeaderText   = options?.HeaderTextColor ?? Colors.White;
        _Surface      = _IsDark ? (options?.SurfaceColorDark ?? options?.SurfaceColor ?? _Md3SurfaceDark)
                                : (options?.SurfaceColor ?? _Md3Surface);
        _OnSurface    = _IsDark ? (options?.OnSurfaceColorDark ?? options?.OnSurfaceColor ?? _Md3OnSurfaceDark)
                                : (options?.OnSurfaceColor ?? _Md3OnSurface);
        _OnSurfaceVar = _IsDark ? (options?.OnSurfaceVariantColorDark ?? options?.OnSurfaceVariantColor ?? _Md3OnSurfaceVarDk)
                                : (options?.OnSurfaceVariantColor ?? _Md3OnSurfaceVar);
        _FontFamily      = options?.FontFamily;
        _UseNepaliScript = options?.UseNepaliScript ?? false;

        _DisplayMode = options?.DisplayMode ?? DateDisplayMode.Both;
        // Wheel is not supported on desktop — fall back to Calendar
        _PickerStyle = options?.PickerStyle == PickerStyle.Wheel ? PickerStyle.Calendar
                                                                 : (options?.PickerStyle ?? PickerStyle.Calendar);
        _IsBsMode    = _DisplayMode != DateDisplayMode.AdOnly;

        var seed = initial ?? BsAdConverter.AdToBs(DateTime.Today);
        _BsYear = seed.Year; _BsMonth = seed.Month; _BsDay = seed.Day;
        _AdDate = BsAdConverter.BsToAd(seed);
        _ViewYear  = _IsBsMode ? _BsYear : _AdDate.Year;
        _ViewMonth = _IsBsMode ? _BsMonth : _AdDate.Month;
        _PickerSelectedYear = _ViewYear;

        bool showHeader   = options?.ShowHeader ?? true;
        string cancelLabel  = options?.CancelLabel  ?? "CANCEL";
        string confirmLabel = options?.ConfirmLabel ?? "OK";
        if (_UseNepaliScript)
        {
            cancelLabel  = "रद्द गर्नुहोस";
            confirmLabel = "ठीक छ";
        }

        // ── Header ────────────────────────────────────────────────────────────
        var selectLabel = new TextBlock
        {
            Text       = UseNepaliGlyphs ? "मिति छान्नुहोस्" : "SELECT DATE",
            FontSize   = 11,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(178, 255, 255, 255)),
        };
        ApplyFont(selectLabel, UseNepaliGlyphs);

        _HeaderDateLabel = new TextBlock
        {
            FontSize   = 22,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(_HeaderText),
            Margin     = new Thickness(0, 4, 0, 2),
        };
        ApplyFont(_HeaderDateLabel, UseNepaliGlyphs);

        _HeaderEquivLabel = new TextBlock
        {
            FontSize   = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(178, 255, 255, 255)),
        };
        ApplyFont(_HeaderEquivLabel, false);

        var headerStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin      = new Thickness(20, 14, 20, 14),
            Children    = { selectLabel, _HeaderDateLabel, _HeaderEquivLabel },
        };
        var headerBorder = new Border
        {
            Background = new SolidColorBrush(_HeaderBg),
            Child      = headerStack,
            IsVisible  = showHeader,
        };

        // ── BS / AD chip toggle ───────────────────────────────────────────────
        _BsChip = BuildChip("BS");
        _AdChip = BuildChip("AD");

        _BsChip.PointerPressed += (_, e) => { e.Handled = true; SetMode(bs: true); };
        _AdChip.PointerPressed += (_, e) => { e.Handled = true; SetMode(bs: false); };

        var chipRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing     = 8,
            Margin      = new Thickness(16, 10, 16, 2),
            IsVisible   = _DisplayMode == DateDisplayMode.Both,
            Children    = { _BsChip, _AdChip },
        };

        // ── Month / year navigation ───────────────────────────────────────────
        _PrevBtn = BuildNavButton("‹");
        _PrevBtn.Click += (_, _) => Navigate(-1);
        _NextBtn = BuildNavButton("›");
        _NextBtn.Click += (_, _) => Navigate(+1);

        _ChevronLabel = new TextBlock
        {
            Text              = "▾",
            FontSize          = 13,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground        = new SolidColorBrush(_OnSurface),
        };

        _MonthYearLabel = new TextBlock
        {
            FontSize          = 14,
            FontWeight        = FontWeight.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground        = new SolidColorBrush(_OnSurface),
        };
        ApplyFont(_MonthYearLabel, UseNepaliGlyphs);

        var monthYearStack = new StackPanel
        {
            Orientation         = Orientation.Horizontal,
            Spacing             = 4,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment   = VerticalAlignment.Center,
            Cursor              = new Cursor(StandardCursorType.Hand),
            Children            = { _MonthYearLabel, _ChevronLabel },
        };
        monthYearStack.PointerPressed += (_, e) => { e.Handled = true; ToggleYearMonthPicker(); };

        var navRow = new Grid { Height = 44, Margin = new Thickness(4, 0) };
        navRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        navRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        navRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        navRow.Children.Add(_PrevBtn);
        Grid.SetColumn(monthYearStack, 1); navRow.Children.Add(monthYearStack);
        Grid.SetColumn(_NextBtn, 2);       navRow.Children.Add(_NextBtn);

        // ── Day-of-week header ────────────────────────────────────────────────
        _DowRow = BuildDowRow();

        // ── Calendar host ─────────────────────────────────────────────────────
        _CalendarHost = new ContentControl { Padding = new Thickness(12, 4, 12, 8) };

        // ── Divider ───────────────────────────────────────────────────────────
        var divider = new Border
        {
            Height     = 1,
            Background = new SolidColorBrush(_IsDark ? Color.Parse("#49454F") : Color.Parse("#E7E0EC")),
        };

        // ── Action buttons ────────────────────────────────────────────────────
        var cancelBtn = new Button
        {
            Content                  = cancelLabel,
            Background               = Brushes.Transparent,
            Foreground               = new SolidColorBrush(_Primary),
            FontSize                 = 14,
            FontWeight               = FontWeight.Bold,
            Padding                  = new Thickness(16, 0),
            Height                   = 40,
            BorderThickness          = new Thickness(0),
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        ApplyFont(cancelBtn, false);
        cancelBtn.Click += (_, _) => Cancelled?.Invoke(this, EventArgs.Empty);

        _OkBtn = new Button
        {
            Content                  = confirmLabel,
            Background               = Brushes.Transparent,
            Foreground               = new SolidColorBrush(_Primary),
            FontSize                 = 14,
            FontWeight               = FontWeight.Bold,
            Padding                  = new Thickness(16, 0),
            Height                   = 40,
            BorderThickness          = new Thickness(0),
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        ApplyFont(_OkBtn, false);
        _OkBtn.Click += (_, _) => CommitAndClose();

        var actionRow = new StackPanel
        {
            Orientation         = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin              = new Thickness(0, 4, 8, 12),
            Children            = { cancelBtn, _OkBtn },
        };

        // ── Root ──────────────────────────────────────────────────────────────
        var root = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Background  = new SolidColorBrush(_Surface),
        };
        root.Children.Add(headerBorder);
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
        if (_PickerStyle == PickerStyle.Input)
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

        if (_PickerMode == PickerMode.YearMonth)
        {
            _PickerMode = PickerMode.Calendar;
            SyncModeUI();
        }

        _PickerSelectedYear = _ViewYear;
        ApplyChipState();
        RefreshHeader();
        if (_PickerStyle == PickerStyle.Input)
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
        var primaryBrush    = new SolidColorBrush(_Primary);
        var onPrimaryBrush  = new SolidColorBrush(_OnPrimary);
        var transparentBrush = Brushes.Transparent;

        _BsChip.Background  = _IsBsMode ? primaryBrush : transparentBrush;
        _AdChip.Background  = _IsBsMode ? transparentBrush : primaryBrush;
        _BsChip.BorderBrush = _IsBsMode ? transparentBrush : primaryBrush;
        _AdChip.BorderBrush = _IsBsMode ? primaryBrush : transparentBrush;

        if (_BsChip.Child is TextBlock bsLbl) bsLbl.Foreground = _IsBsMode ? onPrimaryBrush : primaryBrush;
        if (_AdChip.Child is TextBlock adLbl) adLbl.Foreground = _IsBsMode ? primaryBrush : onPrimaryBrush;
    }

    // ── Year/month picker toggle ──────────────────────────────────────────────

    private void ToggleYearMonthPicker()
    {
        _PickerMode = _PickerMode == PickerMode.Calendar ? PickerMode.YearMonth : PickerMode.Calendar;
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
        RebuildCalendar();
    }

    private void OnMonthTapped(int month)
    {
        _ViewYear  = _PickerSelectedYear;
        _ViewMonth = month;

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
            _CalendarHost.Content = BuildYearMonthPickerView();
        else
            _CalendarHost.Content = BuildCalendarGrid();
    }

    // ── Year + month picker view ──────────────────────────────────────────────

    private Control BuildYearMonthPickerView()
    {
        var root = new StackPanel { Orientation = Orientation.Vertical };

        int minYear   = _IsBsMode ? BsCalendarData.MinYear : 1900;
        int maxYear   = _IsBsMode ? BsCalendarData.MaxYear : 2100;
        int yearCount = maxYear - minYear + 1;
        int yearRows  = (int)Math.Ceiling(yearCount / (double)_YearCols);
        int todayYear = _IsBsMode
            ? BsAdConverter.AdToBs(DateTime.Today).Year
            : DateTime.Today.Year;

        var yearGrid = new Grid();
        for (int c = 0; c < _YearCols; c++)
            yearGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        for (int r = 0; r < yearRows; r++)
            yearGrid.RowDefinitions.Add(new RowDefinition(new GridLength(_YearCellH + _YearSpacing)));

        for (int i = 0; i < yearCount; i++)
        {
            int year = minYear + i;
            var cell = BuildYearCell(year, year == _PickerSelectedYear, year == todayYear);
            cell.Margin = new Thickness(0, 0, _YearSpacing, _YearSpacing);
            Grid.SetRow(cell, i / _YearCols);
            Grid.SetColumn(cell, i % _YearCols);
            int captured = year;
            cell.PointerPressed += (_, e) => { e.Handled = true; OnYearTapped(captured); };
            yearGrid.Children.Add(cell);
        }

        double yearScrollH = _YearVisible * (_YearCellH + _YearSpacing) - _YearSpacing;
        int    selRow      = (_PickerSelectedYear - minYear) / _YearCols;
        double scrollY     = Math.Max(0, (selRow - 1) * (_YearCellH + _YearSpacing));

        var yearScroll = new ScrollViewer
        {
            Content   = yearGrid,
            MaxHeight = yearScrollH,
            Opacity   = 0,
        };
        yearScroll.AttachedToVisualTree += (_, _) =>
            Dispatcher.UIThread.Post(() =>
            {
                yearScroll.Offset  = new Vector(0, scrollY);
                yearScroll.Opacity = 1;
            }, DispatcherPriority.Loaded);

        root.Children.Add(yearScroll);

        // Separator
        var sep = new Border
        {
            Height     = 1,
            Margin     = new Thickness(0, 6),
            Background = new SolidColorBrush(_IsDark ? Color.Parse("#49454F") : Color.Parse("#E7E0EC")),
        };
        root.Children.Add(sep);

        // Month grid (3 × 4)
        string[] monthNames = _IsBsMode
            ? (_UseNepaliScript ? NepaliDate.MonthNamesNepali : NepaliDate.MonthNames)
            : (UseNepaliGlyphs ? NepaliDate.AdMonthNamesNepali : _AdMonthNames);
        bool monthIsDevanagari = _IsBsMode ? _UseNepaliScript : UseNepaliGlyphs;

        var monthGrid = new Grid();
        for (int c = 0; c < _MonthCols; c++)
            monthGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        for (int r = 0; r < 4; r++)
            monthGrid.RowDefinitions.Add(new RowDefinition(new GridLength(_MonthCellH + _MonthSpacing)));

        for (int i = 0; i < 12; i++)
        {
            int  month     = i + 1;
            bool isCurrent = month == _ViewMonth && _PickerSelectedYear == _ViewYear;
            var  cell      = BuildMonthCell(monthNames[i], isCurrent, monthIsDevanagari);
            cell.Margin = new Thickness(0, 0, _MonthSpacing, _MonthSpacing);
            Grid.SetRow(cell, i / _MonthCols);
            Grid.SetColumn(cell, i % _MonthCols);
            int captured = month;
            cell.PointerPressed += (_, e) => { e.Handled = true; OnMonthTapped(captured); };
            monthGrid.Children.Add(cell);
        }

        root.Children.Add(monthGrid);
        return root;
    }

    // ── Year cell ─────────────────────────────────────────────────────────────

    private Control BuildYearCell(int year, bool isSelected, bool isCurrentYear)
    {
        var label = new TextBlock
        {
            Text                = UseNepaliGlyphs ? N(year) : year.ToString(),
            FontSize            = 13,
            TextAlignment       = TextAlignment.Center,
            VerticalAlignment   = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        ApplyFont(label, UseNepaliGlyphs);

        if (isSelected)
        {
            label.Foreground = new SolidColorBrush(_OnPrimary);
            label.FontWeight = FontWeight.Bold;
            return new Border
            {
                Background          = new SolidColorBrush(_Primary),
                CornerRadius        = new CornerRadius(8),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment   = VerticalAlignment.Stretch,
                Cursor              = new Cursor(StandardCursorType.Hand),
                Child               = label,
            };
        }

        if (isCurrentYear)
        {
            label.Foreground = new SolidColorBrush(_Primary);
            label.FontWeight = FontWeight.Bold;
            return new Border
            {
                BorderBrush         = new SolidColorBrush(_Primary),
                BorderThickness     = new Thickness(1),
                CornerRadius        = new CornerRadius(8),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment   = VerticalAlignment.Stretch,
                Cursor              = new Cursor(StandardCursorType.Hand),
                Child               = label,
            };
        }

        label.Foreground = new SolidColorBrush(_OnSurface);
        return new Border
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment   = VerticalAlignment.Stretch,
            Cursor              = new Cursor(StandardCursorType.Hand),
            Child               = label,
        };
    }

    // ── Month cell ────────────────────────────────────────────────────────────

    private Control BuildMonthCell(string name, bool isSelected, bool devanagari = false)
    {
        var label = new TextBlock
        {
            Text                = name,
            FontSize            = 13,
            TextAlignment       = TextAlignment.Center,
            VerticalAlignment   = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        ApplyFont(label, devanagari);

        if (isSelected)
        {
            label.Foreground = new SolidColorBrush(_OnPrimary);
            label.FontWeight = FontWeight.Bold;
            return new Border
            {
                Background          = new SolidColorBrush(_Primary),
                CornerRadius        = new CornerRadius(8),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment   = VerticalAlignment.Stretch,
                Cursor              = new Cursor(StandardCursorType.Hand),
                Child               = label,
            };
        }

        label.Foreground = new SolidColorBrush(_OnSurface);
        return new Border
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment   = VerticalAlignment.Stretch,
            Cursor              = new Cursor(StandardCursorType.Hand),
            Child               = label,
        };
    }

    // ── Calendar grid ─────────────────────────────────────────────────────────

    private Grid BuildCalendarGrid()
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
        var grid = new Grid();
        for (int c = 0; c < 7; c++)
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        for (int r = 0; r < 6; r++)
            grid.RowDefinitions.Add(new RowDefinition(new GridLength(46)));

        int dayNum = 1;
        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < 7; col++)
            {
                if (row * 7 + col < startDow) continue;
                if (dayNum > daysInMonth) break;

                var cell = BuildDayCell(dayNum, dayNum == selectedDay, dayNum == todayDay);
                Grid.SetRow(cell, row);
                Grid.SetColumn(cell, col);
                int captured = dayNum;
                cell.PointerPressed += (_, e) => { e.Handled = true; OnDayTapped(captured); };
                grid.Children.Add(cell);
                dayNum++;
            }
        }

        return grid;
    }

    // ── Input style ───────────────────────────────────────────────────────────

    private void BuildInputField()
    {
        string initialText = _IsBsMode
            ? $"{_BsYear:D4}-{_BsMonth:D2}-{_BsDay:D2}"
            : $"{_AdDate.Year:D4}-{_AdDate.Month:D2}-{_AdDate.Day:D2}";
        _LastInputText = initialText;

        _InputEntry = new TextBox
        {
            Text        = initialText,
            Watermark   = "YYYY-MM-DD",
            MaxLength   = 10,
            FontSize    = 14,
            Background  = Brushes.Transparent,
            Foreground  = new SolidColorBrush(_OnSurface),
        };
        if (_FontFamily is not null) _InputEntry.FontFamily = new FontFamily(_FontFamily);
        _InputEntry.TextChanged += OnInputTextChanged;

        var fieldLabel = new TextBlock
        {
            Text     = _UseNepaliScript ? "मिति लेख्नुहोस्" : "Enter Date",
            FontSize = 10,
            Foreground = new SolidColorBrush(_OnSurfaceVar),
        };
        ApplyFont(fieldLabel, _UseNepaliScript);

        var entryStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children    = { fieldLabel, _InputEntry },
        };

        var entryBorder = new Border
        {
            BorderBrush     = new SolidColorBrush(_Primary),
            BorderThickness = new Thickness(1.5),
            CornerRadius    = new CornerRadius(4),
            Padding         = new Thickness(10, 6),
            Child           = entryStack,
        };

        _CalendarHost.Content = entryBorder;
    }

    private void OnInputTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_FormattingInput || _InputEntry is null) return;

        var newText   = _InputEntry.Text ?? "";
        bool isDeleting = newText.Length < _LastInputText.Length;

        var digits = new string(newText.Where(char.IsDigit).ToArray());
        if (digits.Length > 8) digits = digits[..8];

        string formatted = digits.Length switch
        {
            < 4 => digits,
            4   => isDeleting ? digits : digits + "-",
            < 6 => digits[..4] + "-" + digits[4..],
            6   => isDeleting ? digits[..4] + "-" + digits[4..]
                              : digits[..4] + "-" + digits[4..] + "-",
            _   => digits[..4] + "-" + digits[4..6] + "-" + digits[6..],
        };

        _LastInputText = formatted;

        if (formatted != newText)
        {
            _FormattingInput = true;
            Dispatcher.UIThread.Post(() =>
            {
                _InputEntry!.Text = formatted;
                _FormattingInput  = false;
            });
        }

        ValidateAndApplyInput(formatted);
    }

    private void ValidateAndApplyInput(string text)
    {
        if (_OkBtn is null) return;

        if (text.Length != 10 || text[4] != '-' || text[7] != '-')
        {
            _OkBtn.IsEnabled = false;
            return;
        }

        if (!int.TryParse(text.AsSpan(0, 4), out int year)  ||
            !int.TryParse(text.AsSpan(5, 2), out int month) ||
            !int.TryParse(text.AsSpan(8, 2), out int day))
        {
            _OkBtn.IsEnabled = false;
            return;
        }

        if (_IsBsMode)
        {
            if (year < BsCalendarData.MinYear || year > BsCalendarData.MaxYear || month < 1 || month > 12)
            {
                _OkBtn.IsEnabled = false;
                return;
            }
            int maxDay = BsCalendarData.GetDaysInMonth(year, month);
            if (day < 1 || day > maxDay) { _OkBtn.IsEnabled = false; return; }
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
            if (day < 1 || day > maxDay) { _OkBtn.IsEnabled = false; return; }
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
        _LastInputText = _InputEntry.Text;
    }

    // ── Day cell ──────────────────────────────────────────────────────────────

    private Control BuildDayCell(int day, bool isSelected, bool isToday)
    {
        var label = new TextBlock
        {
            Text                = UseNepaliGlyphs ? N(day) : day.ToString(),
            FontSize            = 14,
            TextAlignment       = TextAlignment.Center,
            VerticalAlignment   = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        ApplyFont(label, UseNepaliGlyphs);

        if (isSelected)
        {
            label.Foreground = new SolidColorBrush(_OnPrimary);
            label.FontWeight = FontWeight.Bold;
            return new Border
            {
                Background          = new SolidColorBrush(_Primary),
                CornerRadius        = new CornerRadius(20),
                Width               = 40,
                Height              = 40,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment   = VerticalAlignment.Center,
                Cursor              = new Cursor(StandardCursorType.Hand),
                Child               = label,
            };
        }

        if (isToday)
        {
            label.Foreground = new SolidColorBrush(_Primary);
            label.FontWeight = FontWeight.Bold;
            return new Border
            {
                BorderBrush         = new SolidColorBrush(_Primary),
                BorderThickness     = new Thickness(1.5),
                CornerRadius        = new CornerRadius(20),
                Width               = 40,
                Height              = 40,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment   = VerticalAlignment.Center,
                Cursor              = new Cursor(StandardCursorType.Hand),
                Child               = label,
            };
        }

        label.Foreground = new SolidColorBrush(_OnSurface);
        return new Border
        {
            Width               = 40,
            Height              = 40,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment   = VerticalAlignment.Center,
            Cursor              = new Cursor(StandardCursorType.Hand),
            Child               = label,
        };
    }

    // ── Day-of-week header row ────────────────────────────────────────────────

    private Grid BuildDowRow()
    {
        var grid = new Grid { Height = 36, Margin = new Thickness(12, 0) };
        for (int i = 0; i < 7; i++)
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        var labels = UseNepaliGlyphs ? _DowLabelsNepali : _DowLabels;
        for (int i = 0; i < 7; i++)
        {
            var lbl = new TextBlock
            {
                Text                = labels[i],
                FontSize            = 13,
                FontWeight          = FontWeight.Bold,
                TextAlignment       = TextAlignment.Center,
                VerticalAlignment   = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Foreground          = new SolidColorBrush(_OnSurfaceVar),
            };
            ApplyFont(lbl, UseNepaliGlyphs);
            Grid.SetColumn(lbl, i);
            grid.Children.Add(lbl);
        }
        return grid;
    }

    private void RefreshDowRow()
    {
        var labels = UseNepaliGlyphs ? _DowLabelsNepali : _DowLabels;
        int i = 0;
        foreach (var child in _DowRow.Children)
        {
            if (child is TextBlock lbl) lbl.Text = labels[i++];
        }
    }

    // ── Builder helpers ───────────────────────────────────────────────────────

    private Border BuildChip(string text)
    {
        var label = new TextBlock
        {
            Text                = text,
            FontSize            = 13,
            FontWeight          = FontWeight.Bold,
            TextAlignment       = TextAlignment.Center,
            VerticalAlignment   = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin              = new Thickness(16, 6),
        };
        ApplyFont(label, false);
        return new Border
        {
            CornerRadius    = new CornerRadius(16),
            BorderThickness = new Thickness(1.5),
            MinWidth        = 64,
            Cursor          = new Cursor(StandardCursorType.Hand),
            Child           = label,
        };
    }

    private Button BuildNavButton(string glyph)
    {
        var btn = new Button
        {
            Content         = glyph,
            FontSize        = 22,
            Background      = Brushes.Transparent,
            Foreground      = new SolidColorBrush(_OnSurface),
            Padding         = new Thickness(8, 0),
            Width           = 44,
            Height          = 44,
            BorderThickness = new Thickness(0),
        };
        return btn;
    }

    private void ApplyFont(TextBlock tb, bool devanagari)
    {
        if (devanagari && _FontFamily is not null)
            tb.FontFamily = new FontFamily(_FontFamily);
    }

    private void ApplyFont(Button btn, bool devanagari)
    {
        if (devanagari && _FontFamily is not null)
            btn.FontFamily = new FontFamily(_FontFamily);
    }

    private static string N(int n)
    {
        return n.ToString()
            .Replace('0', '०').Replace('1', '१').Replace('2', '२')
            .Replace('3', '३').Replace('4', '४').Replace('5', '५')
            .Replace('6', '६').Replace('7', '७').Replace('8', '८')
            .Replace('9', '९');
    }
}

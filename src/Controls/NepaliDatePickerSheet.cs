using Microsoft.Maui.Controls.Shapes;
using nepali_calendar_picker.Data;
using nepali_calendar_picker.Models;
using nepali_calendar_picker.Services;

namespace nepali_calendar_picker.Controls
{
    /// <summary>
    /// Material Design 3 calendar date-picker sheet.
    /// Layout: drag handle → colored header → BS/AD chip toggle (optional) →
    ///         month navigation → day-of-week headers → calendar grid → CANCEL / OK actions.
    /// Tapping the month/year label toggles a year + month grid for fast navigation.
    /// </summary>
    internal class NepaliDatePickerSheet : ContentView
    {
        // ── Picker sub-mode ───────────────────────────────────────────────────────
        private enum PickerMode { Calendar, YearMonth }

        // ── MD3 static defaults ───────────────────────────────────────────────────
        private static readonly Color Md3Primary = Color.FromArgb("#6750A4");
        private static readonly Color Md3PrimaryDark = Color.FromArgb("#D0BCFF");
        private static readonly Color Md3OnPrimary = Colors.White;
        private static readonly Color Md3OnPrimaryDark = Color.FromArgb("#381E72");
        private static readonly Color Md3Surface = Color.FromArgb("#FFFBFE");
        private static readonly Color Md3SurfaceDark = Color.FromArgb("#1C1B1F");
        private static readonly Color Md3OnSurface = Color.FromArgb("#1C1B1F");
        private static readonly Color Md3OnSurfaceDark = Color.FromArgb("#E6E1E5");
        private static readonly Color Md3OnSurfaceVar = Color.FromArgb("#49454F");
        private static readonly Color Md3OnSurfaceVarDk = Color.FromArgb("#CAC4D0");
        private static readonly Color Md3HeaderBgLight = Color.FromArgb("#6750A4");
        private static readonly Color Md3HeaderBgDark = Color.FromArgb("#4A4458");

        // ── Effective colors ──────────────────────────────────────────────────────
        private readonly Color _primary, _primaryDark;
        private readonly Color _onPrimary, _onPrimaryDark;
        private readonly Color _headerBg, _headerBgDark;
        private readonly Color _headerText;
        private readonly Color _surface, _surfaceDark;
        private readonly Color _onSurface, _onSurfaceDark;
        private readonly Color _onSurfaceVar, _onSurfaceVarDk;
        private readonly string? _fontFamily;

        // ── Events ────────────────────────────────────────────────────────────────
        public event EventHandler<NepaliDate>? Done;
        public event EventHandler? Cancelled;

        // ── Selection state ───────────────────────────────────────────────────────
        private bool _isBsMode;
        private int _bsYear, _bsMonth, _bsDay;
        private DateTime _adDate;
        private readonly DateDisplayMode _displayMode;

        // ── View state ────────────────────────────────────────────────────────────
        private int _viewYear, _viewMonth;

        // ── Year/month picker state ───────────────────────────────────────────────
        private PickerMode _pickerMode = PickerMode.Calendar;
        private int _pickerSelectedYear;

        // ── Mutable UI references ─────────────────────────────────────────────────
        private readonly Label _headerDateLabel;
        private readonly Label _headerEquivLabel;
        private readonly Label _monthYearLabel;
        private readonly Label _chevronLabel;
        private readonly Button _prevBtn;
        private readonly Button _nextBtn;
        private readonly Grid _dowRow;
        private readonly ContentView _calendarHost;
        private readonly Border _bsChip, _adChip;

        private static readonly string[] DowLabels = ["S", "M", "T", "W", "T", "F", "S"];
        private static readonly string[] AdMonthNames =
        [
            "January","February","March","April","May","June",
        "July","August","September","October","November","December"
        ];

        // ── Layout constants for year/month picker ────────────────────────────────
        private const int YearCellH = 36;
        private const int YearSpacing = 2;
        private const int YearCols = 3;
        private const int YearVisible = 4;   // rows visible in scroll
        private const int MonthCellH = 44;
        private const int MonthSpacing = 2;
        private const int MonthCols = 3;

        public NepaliDatePickerSheet(NepaliDate? initial = null, NepaliDatePickerOptions? options = null)
        {
            // ── Resolve effective colors ──────────────────────────────────────────
            _primary = options?.PrimaryColor ?? Md3Primary;
            _primaryDark = options?.PrimaryColorDark ?? options?.PrimaryColor ?? Md3PrimaryDark;
            _onPrimary = options?.OnPrimaryColor ?? Md3OnPrimary;
            _onPrimaryDark = options?.OnPrimaryColor ?? Md3OnPrimaryDark;
            _headerBg = options?.HeaderBackgroundColor ?? Md3HeaderBgLight;
            _headerBgDark = options?.HeaderBackgroundColorDark ?? options?.HeaderBackgroundColor ?? Md3HeaderBgDark;
            _headerText = options?.HeaderTextColor ?? Colors.White;
            _surface = options?.SurfaceColor ?? Md3Surface;
            _surfaceDark = options?.SurfaceColorDark ?? options?.SurfaceColor ?? Md3SurfaceDark;
            _onSurface = options?.OnSurfaceColor ?? Md3OnSurface;
            _onSurfaceDark = options?.OnSurfaceColorDark ?? options?.OnSurfaceColor ?? Md3OnSurfaceDark;
            _onSurfaceVar = options?.OnSurfaceVariantColor ?? Md3OnSurfaceVar;
            _onSurfaceVarDk = options?.OnSurfaceVariantColorDark ?? options?.OnSurfaceVariantColor ?? Md3OnSurfaceVarDk;
            _fontFamily = options?.FontFamily;

            _displayMode = options?.DisplayMode ?? DateDisplayMode.Both;
            _isBsMode = _displayMode != DateDisplayMode.AdOnly;

            var seed = initial ?? BsAdConverter.AdToBs(DateTime.Today);
            _bsYear = seed.Year; _bsMonth = seed.Month; _bsDay = seed.Day;
            _adDate = BsAdConverter.BsToAd(seed);
            _viewYear = _isBsMode ? _bsYear : _adDate.Year;
            _viewMonth = _isBsMode ? _bsMonth : _adDate.Month;
            _pickerSelectedYear = _viewYear;

            // ── Drag handle (bottom-sheet only) ──────────────────────────────────
            bool isDialog = (options?.Presentation ?? PickerPresentation.BottomSheet) == PickerPresentation.Dialog;
            var handle = new BoxView
            {
                HeightRequest = 4,
                WidthRequest = 32,
                CornerRadius = 2,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 10, 0, 0),
                IsVisible = !isDialog,
            };
            handle.SetAppThemeColor(BoxView.ColorProperty,
                Color.FromArgb("#CCCCCC"), Color.FromArgb("#49454F"));

            // ── Header ────────────────────────────────────────────────────────────
            var selectLabel = new Label
            {
                Text = "SELECT DATE",
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                CharacterSpacing = 1.5,
                TextColor = Color.FromRgba((byte)255, (byte)255, (byte)255, (byte)178),
            };
            ApplyFont(selectLabel);

            _headerDateLabel = new Label
            {
                FontSize = 22,
                FontAttributes = FontAttributes.Bold,
                TextColor = _headerText,
            };
            ApplyFont(_headerDateLabel);

            _chevronLabel = new Label
            {
                Text = "▾",
                FontSize = 13,
                VerticalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(6, 0, 0, 0),
                TextColor = Color.FromRgba((byte)255, (byte)255, (byte)255, (byte)178),
            };

            var headerDateRow = new HorizontalStackLayout
            {
                Spacing = 0,
                Margin = new Thickness(0, 4, 0, 2),
                Children = { _headerDateLabel, _chevronLabel },
            };
            var headerTap = new TapGestureRecognizer();
            headerTap.Tapped += (_, _) => ToggleYearMonthPicker();
            headerDateRow.GestureRecognizers.Add(headerTap);

            _headerEquivLabel = new Label
            {
                FontSize = 12,
                TextColor = Color.FromRgba((byte)255, (byte)255, (byte)255, (byte)178),
            };
            ApplyFont(_headerEquivLabel);

            var headerContent = new VerticalStackLayout
            {
                Padding = new Thickness(20, 14, 20, 14),
                Spacing = 0,
                Children = { selectLabel, headerDateRow, _headerEquivLabel },
            };
            headerContent.SetAppThemeColor(VisualElement.BackgroundColorProperty, _headerBg, _headerBgDark);

            // ── BS / AD chip toggle ───────────────────────────────────────────────
            _bsChip = BuildChip("BS");
            _adChip = BuildChip("AD");

            var bsTap = new TapGestureRecognizer();
            bsTap.Tapped += (_, _) => SetMode(bs: true);
            _bsChip.GestureRecognizers.Add(bsTap);

            var adTap = new TapGestureRecognizer();
            adTap.Tapped += (_, _) => SetMode(bs: false);
            _adChip.GestureRecognizers.Add(adTap);

            var chipRow = new HorizontalStackLayout
            {
                Spacing = 8,
                Padding = new Thickness(16, 10, 16, 2),
                IsVisible = _displayMode == DateDisplayMode.Both,
                Children = { _bsChip, _adChip },
            };

            // ── Month / year navigation ───────────────────────────────────────────
            _prevBtn = BuildNavButton("‹");
            _prevBtn.Clicked += (_, _) => Navigate(-1);
            _nextBtn = BuildNavButton("›");
            _nextBtn.Clicked += (_, _) => Navigate(+1);

            _monthYearLabel = new Label
            {
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.Fill,
            };
            _monthYearLabel.SetAppThemeColor(Label.TextColorProperty, _onSurface, _onSurfaceDark);
            ApplyFont(_monthYearLabel);

            var navRow = new Grid { HeightRequest = 44, Padding = new Thickness(4, 0) };
            navRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            navRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            navRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            navRow.Add(_prevBtn);
            Grid.SetColumn(_monthYearLabel, 1); navRow.Add(_monthYearLabel);
            Grid.SetColumn(_nextBtn, 2); navRow.Add(_nextBtn);

            // ── Day-of-week header ────────────────────────────────────────────────
            _dowRow = BuildDowRow();

            // ── Calendar grid host ────────────────────────────────────────────────
            _calendarHost = new ContentView { Padding = new Thickness(12, 4, 12, 8) };

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
            cancelBtn.SetAppThemeColor(Button.TextColorProperty, _primary, _primaryDark);
            if (_fontFamily is not null) cancelBtn.FontFamily = _fontFamily;
            cancelBtn.Clicked += (_, _) => Cancelled?.Invoke(this, EventArgs.Empty);

            var okBtn = new Button
            {
                Text = "OK",
                BackgroundColor = Colors.Transparent,
                BorderWidth = 0,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                Padding = new Thickness(16, 0),
                HeightRequest = 40,
            };
            okBtn.SetAppThemeColor(Button.TextColorProperty, _primary, _primaryDark);
            if (_fontFamily is not null) okBtn.FontFamily = _fontFamily;
            okBtn.Clicked += (_, _) => CommitAndClose();

            var actionRow = new HorizontalStackLayout
            {
                HorizontalOptions = LayoutOptions.End,
                Padding = new Thickness(0, 4, 8, 12),
                Spacing = 0,
                Children = { cancelBtn, okBtn },
            };

            // ── Root ──────────────────────────────────────────────────────────────
            var root = new VerticalStackLayout { Spacing = 0 };
            root.SetAppThemeColor(VisualElement.BackgroundColorProperty, _surface, _surfaceDark);
            root.Children.Add(handle);
            root.Children.Add(headerContent);
            root.Children.Add(chipRow);
            root.Children.Add(navRow);
            root.Children.Add(_dowRow);
            root.Children.Add(_calendarHost);
            root.Children.Add(divider);
            root.Children.Add(actionRow);

            Content = root;

            ApplyChipState();
            RefreshHeader();
            RefreshMonthYear();
            RebuildCalendar();
        }

        // ── BS / AD mode switching ────────────────────────────────────────────────

        private void SetMode(bool bs)
        {
            if (_displayMode != DateDisplayMode.Both) return;
            if (_isBsMode == bs) return;
            _isBsMode = bs;

            if (bs)
            {
                var synced = BsAdConverter.AdToBs(_adDate);
                _bsYear = synced.Year; _bsMonth = synced.Month; _bsDay = synced.Day;
                _viewYear = _bsYear; _viewMonth = _bsMonth;
            }
            else
            {
                _adDate = BsAdConverter.BsToAd(new NepaliDate(_bsYear, _bsMonth, _bsDay));
                _viewYear = _adDate.Year; _viewMonth = _adDate.Month;
            }

            // Exit year/month picker when switching calendar system
            if (_pickerMode == PickerMode.YearMonth)
            {
                _pickerMode = PickerMode.Calendar;
                SyncModeUI();
            }

            _pickerSelectedYear = _viewYear;
            ApplyChipState();
            RefreshHeader();
            RefreshMonthYear();
            RebuildCalendar();
        }

        private void ApplyChipState()
        {
            _bsChip.BackgroundColor = _isBsMode ? _primary : Colors.Transparent;
            _adChip.BackgroundColor = _isBsMode ? Colors.Transparent : _primary;

            _bsChip.Stroke = _isBsMode ? Colors.Transparent : new SolidColorBrush(_primary);
            _adChip.Stroke = _isBsMode ? new SolidColorBrush(_primary) : Colors.Transparent;

            if (_bsChip.Content is Label bsLbl) bsLbl.TextColor = _isBsMode ? _onPrimary : _primary;
            if (_adChip.Content is Label adLbl) adLbl.TextColor = _isBsMode ? _primary : _onPrimary;
        }

        // ── Year/month picker toggle ──────────────────────────────────────────────

        private void ToggleYearMonthPicker()
        {
            _pickerMode = _pickerMode == PickerMode.Calendar
                ? PickerMode.YearMonth
                : PickerMode.Calendar;

            if (_pickerMode == PickerMode.YearMonth)
                _pickerSelectedYear = _viewYear;

            SyncModeUI();
            RebuildCalendar();
        }

        private void SyncModeUI()
        {
            bool isCalendar = _pickerMode == PickerMode.Calendar;
            _chevronLabel.Text = isCalendar ? "▾" : "▴";
            _prevBtn.IsVisible = isCalendar;
            _nextBtn.IsVisible = isCalendar;
            _dowRow.IsVisible = isCalendar;
        }

        // ── Navigation ────────────────────────────────────────────────────────────

        private void Navigate(int delta)
        {
            _viewMonth += delta;
            if (_viewMonth < 1) { _viewMonth = 12; _viewYear--; }
            else if (_viewMonth > 12) { _viewMonth = 1; _viewYear++; }

            _viewYear = _isBsMode
                ? Math.Clamp(_viewYear, BsCalendarData.MinYear, BsCalendarData.MaxYear)
                : Math.Clamp(_viewYear, 1900, 2100);

            RefreshMonthYear();
            RebuildCalendar();
        }

        // ── Day tapped ────────────────────────────────────────────────────────────

        private void OnDayTapped(int day)
        {
            if (_isBsMode)
            {
                _bsYear = _viewYear; _bsMonth = _viewMonth; _bsDay = day;
                _adDate = BsAdConverter.BsToAd(new NepaliDate(_bsYear, _bsMonth, _bsDay));
            }
            else
            {
                _adDate = new DateTime(_viewYear, _viewMonth, day);
                var bs = BsAdConverter.AdToBs(_adDate);
                _bsYear = bs.Year; _bsMonth = bs.Month; _bsDay = bs.Day;
            }

            RefreshHeader();
            RebuildCalendar();
        }

        // ── Year/month picker handlers ────────────────────────────────────────────

        private void OnYearTapped(int year)
        {
            _pickerSelectedYear = year;
            RebuildCalendar(); // re-render year grid with new highlight; scroll preserved via Loaded
        }

        private void OnMonthTapped(int month)
        {
            _viewYear = _pickerSelectedYear;
            _viewMonth = month;

            // Clamp current selection into the new year+month
            if (_isBsMode)
            {
                int maxDay = BsCalendarData.GetDaysInMonth(_viewYear, _viewMonth);
                _bsDay = Math.Min(_bsDay, maxDay);
                _bsYear = _viewYear;
                _bsMonth = _viewMonth;
                _adDate = BsAdConverter.BsToAd(new NepaliDate(_bsYear, _bsMonth, _bsDay));
            }
            else
            {
                int maxDay = DateTime.DaysInMonth(_viewYear, _viewMonth);
                int day = Math.Min(_adDate.Day, maxDay);
                _adDate = new DateTime(_viewYear, _viewMonth, day);
                var bs = BsAdConverter.AdToBs(_adDate);
                _bsYear = bs.Year; _bsMonth = bs.Month; _bsDay = bs.Day;
            }

            _pickerMode = PickerMode.Calendar;
            SyncModeUI();
            RefreshHeader();
            RefreshMonthYear();
            RebuildCalendar();
        }

        private void CommitAndClose()
            => Done?.Invoke(this, new NepaliDate(_bsYear, _bsMonth, _bsDay));

        // ── Header & month label ──────────────────────────────────────────────────

        private void RefreshHeader()
        {
            DateTime ad = BsAdConverter.BsToAd(new NepaliDate(_bsYear, _bsMonth, _bsDay));

            if (_isBsMode)
            {
                _headerDateLabel.Text = $"{ad:ddd}, {NepaliDate.MonthNames[_bsMonth - 1]} {_bsDay}, {_bsYear}";
                _headerEquivLabel.Text = _displayMode == DateDisplayMode.BsOnly
                    ? string.Empty
                    : $"AD  {ad:d MMMM yyyy}";
            }
            else
            {
                _headerDateLabel.Text = $"{_adDate:ddd, MMMM d, yyyy}";
                _headerEquivLabel.Text = _displayMode == DateDisplayMode.AdOnly
                    ? string.Empty
                    : $"BS  {new NepaliDate(_bsYear, _bsMonth, _bsDay).ToDisplayString()}";
            }
        }

        private void RefreshMonthYear()
        {
            _monthYearLabel.Text = _isBsMode
                ? $"{NepaliDate.MonthNames[_viewMonth - 1]}  {_viewYear}"
                : $"{new DateTime(_viewYear, _viewMonth, 1):MMMM yyyy}";
        }

        // ── Calendar / year-month dispatch ────────────────────────────────────────

        private void RebuildCalendar()
        {
            if (_pickerMode == PickerMode.YearMonth)
            {
                _calendarHost.Content = BuildYearMonthPickerView();
                _calendarHost.HeightRequest = YearMonthPickerHeight();
            }
            else
            {
                var (grid, _) = BuildCalendarGrid();
                _calendarHost.Content = grid;
                _calendarHost.HeightRequest = 6 * 44 + 5 * 2 + 12; // fixed 6-row height = 286
            }
        }

        private static double YearMonthPickerHeight()
        {
            // year scroll + separator (1 + 6+6 margin) + month grid + calendarHost padding (4+8)
            double yearScroll = YearVisible * (YearCellH + YearSpacing) - YearSpacing;  // 150
            const double sep = 1 + 6 + 6;                                               //  13
            double monthGrid = 4 * MonthCellH + 3 * MonthSpacing;                       // 182
            return yearScroll + sep + monthGrid + 12;                                      // 357
        }

        // ── Year + month picker view ──────────────────────────────────────────────

        private View BuildYearMonthPickerView()
        {
            var root = new VerticalStackLayout { Spacing = 0 };

            // ── Year grid (scrollable) ────────────────────────────────────────────
            int minYear = _isBsMode ? BsCalendarData.MinYear : 1900;
            int maxYear = _isBsMode ? BsCalendarData.MaxYear : 2100;
            int yearCount = maxYear - minYear + 1;
            int yearRows = (int)Math.Ceiling(yearCount / (double)YearCols);
            int todayYear = _isBsMode
                ? BsAdConverter.AdToBs(DateTime.Today).Year
                : DateTime.Today.Year;

            var yearGrid = new Grid { RowSpacing = YearSpacing, ColumnSpacing = YearSpacing };
            for (int c = 0; c < YearCols; c++)
                yearGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            for (int r = 0; r < yearRows; r++)
                yearGrid.RowDefinitions.Add(new RowDefinition(new GridLength(YearCellH)));

            for (int i = 0; i < yearCount; i++)
            {
                int year = minYear + i;
                var cell = BuildYearCell(year, year == _pickerSelectedYear, year == todayYear);
                Grid.SetRow(cell, i / YearCols);
                Grid.SetColumn(cell, i % YearCols);

                int captured = year;
                var tap = new TapGestureRecognizer();
                tap.Tapped += (_, _) => OnYearTapped(captured);
                cell.GestureRecognizers.Add(tap);
                yearGrid.Add(cell);
            }

            double yearScrollH = YearVisible * (YearCellH + YearSpacing) - YearSpacing;
            int selRow = (_pickerSelectedYear - minYear) / YearCols;
            double scrollY = Math.Max(0, (selRow - 1.5) * (YearCellH + YearSpacing));

            var yearScroll = new ScrollView
            {
                Content = yearGrid,
                HeightRequest = yearScrollH,
            };
            yearScroll.Loaded += async (_, _) => await yearScroll.ScrollToAsync(0, scrollY, false);

            root.Children.Add(yearScroll);

            // ── Separator ─────────────────────────────────────────────────────────
            var sep = new BoxView { HeightRequest = 1, Margin = new Thickness(0, 6) };
            sep.SetAppThemeColor(BoxView.ColorProperty,
                Color.FromArgb("#E7E0EC"), Color.FromArgb("#49454F"));
            root.Children.Add(sep);

            // ── Month grid (3 × 4) ────────────────────────────────────────────────
            string[] monthNames = _isBsMode ? NepaliDate.MonthNames : AdMonthNames;

            var monthGrid = new Grid { RowSpacing = MonthSpacing, ColumnSpacing = MonthSpacing };
            for (int c = 0; c < MonthCols; c++)
                monthGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            for (int r = 0; r < 4; r++)
                monthGrid.RowDefinitions.Add(new RowDefinition(new GridLength(MonthCellH)));

            for (int i = 0; i < 12; i++)
            {
                int month = i + 1;
                bool isCurrent = month == _viewMonth && _pickerSelectedYear == _viewYear;
                var cell = BuildMonthCell(monthNames[i], isCurrent);
                Grid.SetRow(cell, i / MonthCols);
                Grid.SetColumn(cell, i % MonthCols);

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
                Text = year.ToString(),
                FontSize = 13,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
            };
            ApplyFont(label);

            if (isSelected)
            {
                label.TextColor = _onPrimary;
                label.FontAttributes = FontAttributes.Bold;
                return new Border
                {
                    BackgroundColor = _primary,
                    Stroke = Colors.Transparent,
                    StrokeThickness = 0,
                    StrokeShape = new RoundRectangle { CornerRadius = 8 },
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill,
                    Content = label,
                };
            }

            if (isCurrentYear)
            {
                label.TextColor = _primary;
                label.FontAttributes = FontAttributes.Bold;
                return new Border
                {
                    BackgroundColor = Colors.Transparent,
                    Stroke = new SolidColorBrush(_primary),
                    StrokeThickness = 1,
                    StrokeShape = new RoundRectangle { CornerRadius = 8 },
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill,
                    Content = label,
                };
            }

            label.SetAppThemeColor(Label.TextColorProperty, _onSurface, _onSurfaceDark);
            return label;
        }

        // ── Month cell ────────────────────────────────────────────────────────────

        private View BuildMonthCell(string name, bool isSelected)
        {
            var label = new Label
            {
                Text = name,
                FontSize = 13,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
            };
            ApplyFont(label);

            if (isSelected)
            {
                label.TextColor = _onPrimary;
                label.FontAttributes = FontAttributes.Bold;
                return new Border
                {
                    BackgroundColor = _primary,
                    Stroke = Colors.Transparent,
                    StrokeThickness = 0,
                    StrokeShape = new RoundRectangle { CornerRadius = 8 },
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill,
                    Content = label,
                };
            }

            label.SetAppThemeColor(Label.TextColorProperty, _onSurface, _onSurfaceDark);
            return label;
        }

        // ── Calendar grid ─────────────────────────────────────────────────────────

        private (Grid grid, int rowCount) BuildCalendarGrid()
        {
            int daysInMonth, startDow, todayDay = -1, selectedDay = -1;

            if (_isBsMode)
            {
                daysInMonth = BsCalendarData.GetDaysInMonth(_viewYear, _viewMonth);
                startDow = (int)BsAdConverter.BsToAd(new NepaliDate(_viewYear, _viewMonth, 1)).DayOfWeek;

                var todayBs = BsAdConverter.AdToBs(DateTime.Today);
                if (todayBs.Year == _viewYear && todayBs.Month == _viewMonth)
                    todayDay = todayBs.Day;

                if (_bsYear == _viewYear && _bsMonth == _viewMonth)
                    selectedDay = _bsDay;
            }
            else
            {
                daysInMonth = DateTime.DaysInMonth(_viewYear, _viewMonth);
                startDow = (int)new DateTime(_viewYear, _viewMonth, 1).DayOfWeek;

                if (DateTime.Today.Year == _viewYear && DateTime.Today.Month == _viewMonth)
                    todayDay = DateTime.Today.Day;

                if (_adDate.Year == _viewYear && _adDate.Month == _viewMonth)
                    selectedDay = _adDate.Day;
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
                    bool isToday = dayNum == todayDay;

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

        // ── Day cell ──────────────────────────────────────────────────────────────

        private View BuildDayCell(int day, bool isSelected, bool isToday)
        {
            var label = new Label
            {
                Text = day.ToString(),
                FontSize = 14,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
            };
            ApplyFont(label);

            if (isSelected)
            {
                label.TextColor = _onPrimary;
                label.FontAttributes = FontAttributes.Bold;
                return new Border
                {
                    BackgroundColor = _primary,
                    Stroke = Colors.Transparent,
                    StrokeThickness = 0,
                    StrokeShape = new RoundRectangle { CornerRadius = 20 },
                    WidthRequest = 40,
                    HeightRequest = 40,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Content = label,
                };
            }

            if (isToday)
            {
                label.TextColor = _primary;
                label.FontAttributes = FontAttributes.Bold;
                return new Border
                {
                    BackgroundColor = Colors.Transparent,
                    Stroke = new SolidColorBrush(_primary),
                    StrokeThickness = 1.5,
                    StrokeShape = new RoundRectangle { CornerRadius = 20 },
                    WidthRequest = 40,
                    HeightRequest = 40,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Content = label,
                };
            }

            label.SetAppThemeColor(Label.TextColorProperty, _onSurface, _onSurfaceDark);
            return label;
        }

        // ── Day-of-week header row ────────────────────────────────────────────────

        private Grid BuildDowRow()
        {
            var grid = new Grid { HeightRequest = 36, Padding = new Thickness(12, 0) };
            for (int i = 0; i < 7; i++)
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            for (int i = 0; i < 7; i++)
            {
                var lbl = new Label
                {
                    Text = DowLabels[i],
                    FontSize = 12,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                };
                lbl.SetAppThemeColor(Label.TextColorProperty, _onSurfaceVar, _onSurfaceVarDk);
                ApplyFont(lbl);
                Grid.SetColumn(lbl, i);
                grid.Add(lbl);
            }
            return grid;
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
                VerticalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(16, 6),
            };
            ApplyFont(label);
            return new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 16 },
                StrokeThickness = 1.5,
                Padding = 0,
                MinimumWidthRequest = 64,
                Content = label,
            };
        }

        private Button BuildNavButton(string glyph)
        {
            var btn = new Button
            {
                Text = glyph,
                FontSize = 22,
                BackgroundColor = Colors.Transparent,
                BorderWidth = 0,
                Padding = new Thickness(8, 0),
                WidthRequest = 44,
                HeightRequest = 44,
            };
            btn.SetAppThemeColor(Button.TextColorProperty, _onSurface, _onSurfaceDark);
            if (_fontFamily is not null) btn.FontFamily = _fontFamily;
            return btn;
        }

        private void ApplyFont(Label label)
        {
            if (_fontFamily is not null) label.FontFamily = _fontFamily;
        }
    }
}

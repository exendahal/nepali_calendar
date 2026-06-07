namespace nepali_calendar_picker.Controls
{
    /// <summary>
    /// A vertical drum-roll (slot-machine) picker that shows 5 items at a time.
    /// The center item is selected; items fade toward the edges for a native feel.
    /// Scroll debounces and snaps to the nearest item automatically.
    /// </summary>
    public class DrumRollPicker : ContentView
    {
        // ── Layout constants ────────────────────────────────────────────────────
        private const double ItemHeight = 44;
        private const int VisibleItems = 5;   // must be odd
        private const int PaddingItems = VisibleItems / 2;  // = 2 phantom items each end
        private const double TotalHeight = ItemHeight * VisibleItems;

        // ── Bindable properties ─────────────────────────────────────────────────
        public static readonly BindableProperty ItemsProperty =
            BindableProperty.Create(nameof(Items), typeof(IReadOnlyList<string>),
                typeof(DrumRollPicker), Array.Empty<string>(),
                propertyChanged: (b, _, n) => ((DrumRollPicker)b).OnItemsChanged((IReadOnlyList<string>)n));

        public static readonly BindableProperty SelectedIndexProperty =
            BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(DrumRollPicker), 0,
                BindingMode.TwoWay,
                propertyChanged: (b, o, n) => ((DrumRollPicker)b).OnSelectedIndexChanged((int)o, (int)n));

        public IReadOnlyList<string> Items
        {
            get => (IReadOnlyList<string>)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public event EventHandler<int>? SelectionChanged;

        // ── Private state ────────────────────────────────────────────────────────
        private readonly ScrollView _scroll;
        private readonly VerticalStackLayout _stack;
        private readonly List<Label> _itemLabels = [];
        private IDispatcherTimer? _snapTimer;
        private bool _programmaticScroll;

        public DrumRollPicker()
        {
            HeightRequest = TotalHeight;

            _stack = new VerticalStackLayout { Spacing = 0 };

            _scroll = new ScrollView
            {
                Content = _stack,
                Orientation = ScrollOrientation.Vertical,
                VerticalScrollBarVisibility = ScrollBarVisibility.Never,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
            };
            _scroll.Scrolled += OnScrolled;

            // Selection band: two thin separator lines around the centre item
            var topRule = new BoxView
            {
                HeightRequest = 1,
                BackgroundColor = Colors.Transparent,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, ItemHeight * PaddingItems, 0, 0),
                InputTransparent = true,
            };
            topRule.SetAppThemeColor(BoxView.ColorProperty, Color.FromArgb("#40000000"), Color.FromArgb("#40FFFFFF"));

            var bottomRule = new BoxView
            {
                HeightRequest = 1,
                BackgroundColor = Colors.Transparent,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, ItemHeight * (PaddingItems + 1), 0, 0),
                InputTransparent = true,
            };
            bottomRule.SetAppThemeColor(BoxView.ColorProperty, Color.FromArgb("#40000000"), Color.FromArgb("#40FFFFFF"));

            var topFade = BuildFadeOverlay(isTop: true);
            var bottomFade = BuildFadeOverlay(isTop: false);

            var overlay = new Grid { InputTransparent = true };
            overlay.Add(topRule);
            overlay.Add(bottomRule);
            overlay.Add(topFade);
            overlay.Add(bottomFade);

            var root = new Grid();
            root.Add(_scroll);
            root.Add(overlay);

            Content = root;
        }

        // ── Item construction ────────────────────────────────────────────────────

        private void OnItemsChanged(IReadOnlyList<string> items)
        {
            _stack.Children.Clear();
            _itemLabels.Clear();

            for (int i = 0; i < PaddingItems; i++)
                _stack.Children.Add(MakePhantomItem());

            for (int i = 0; i < items.Count; i++)
            {
                var label = MakeItemLabel(items[i]);
                _itemLabels.Add(label);
                _stack.Children.Add(label);
            }

            for (int i = 0; i < PaddingItems; i++)
                _stack.Children.Add(MakePhantomItem());

            // Fire-and-forget scroll to selected position after layout
            Dispatcher.Dispatch(async () => await ScrollTo(SelectedIndex, animated: false));
        }

        private static Label MakeItemLabel(string text) => new()
        {
            Text = text,
            HeightRequest = ItemHeight,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            FontSize = 16,
            Opacity = 0.4,
        };

        private static BoxView MakePhantomItem() => new()
        {
            HeightRequest = ItemHeight,
            Color = Colors.Transparent,
        };

        // ── Scroll handling ──────────────────────────────────────────────────────

        private void OnScrolled(object? sender, ScrolledEventArgs e)
        {
            if (_programmaticScroll) return;
            UpdateItemAppearances(e.ScrollY);
            StartSnapTimer();
        }

        private void StartSnapTimer()
        {
            _snapTimer?.Stop();
            _snapTimer = Dispatcher.CreateTimer();
            _snapTimer.Interval = TimeSpan.FromMilliseconds(160);
            _snapTimer.IsRepeating = false;
            _snapTimer.Tick += OnSnapTimerTick;
            _snapTimer.Start();
        }

        private async void OnSnapTimerTick(object? sender, EventArgs e)
        {
            _snapTimer?.Stop();
            await SnapAsync();
        }

        private async Task SnapAsync()
        {
            double rawIndex = _scroll.ScrollY / ItemHeight;
            int nearestIndex = (int)Math.Round(rawIndex);
            nearestIndex = Math.Clamp(nearestIndex, 0, Items.Count - 1);

            await ScrollTo(nearestIndex, animated: true);

            if (SelectedIndex != nearestIndex)
            {
                _programmaticScroll = true;
                SelectedIndex = nearestIndex;
                _programmaticScroll = false;
                SelectionChanged?.Invoke(this, nearestIndex);
            }

            UpdateItemAppearances(nearestIndex * ItemHeight);
        }

        private async Task ScrollTo(int index, bool animated)
        {
            if (Items.Count == 0) return;
            index = Math.Clamp(index, 0, Items.Count - 1);
            double targetY = index * ItemHeight;

            _programmaticScroll = true;
            await _scroll.ScrollToAsync(0, targetY, animated);
            _programmaticScroll = false;

            UpdateItemAppearances(targetY);
        }

        private void OnSelectedIndexChanged(int oldIndex, int newIndex)
        {
            if (_programmaticScroll) return;
            if (Items.Count == 0) return;
            Dispatcher.Dispatch(async () => await ScrollTo(newIndex, animated: true));
        }

        // ── Visual feedback during scroll ────────────────────────────────────────

        private void UpdateItemAppearances(double scrollY)
        {
            double centerItemIndex = scrollY / ItemHeight;

            for (int i = 0; i < _itemLabels.Count; i++)
            {
                double distance = Math.Abs(i - centerItemIndex);
                var label = _itemLabels[i];

                label.Opacity = distance switch
                {
                    < 0.5 => 1.0,
                    < 1.5 => 0.65,
                    < 2.5 => 0.35,
                    _ => 0.2,
                };

                label.FontSize = distance < 0.5 ? 18 : 15;
                label.FontAttributes = distance < 0.5 ? FontAttributes.Bold : FontAttributes.None;
            }
        }

        // ── Fade overlay helpers ─────────────────────────────────────────────────

        private static View BuildFadeOverlay(bool isTop)
        {
            var stack = new VerticalStackLayout
            {
                Spacing = 0,
                VerticalOptions = isTop ? LayoutOptions.Start : LayoutOptions.End,
                HeightRequest = ItemHeight * PaddingItems,
                InputTransparent = true,
            };

            // alpha values: stronger at the edge, lighter near the centre
            double[] alphas = isTop ? [0.55, 0.30] : [0.30, 0.55];

            foreach (double alpha in alphas)
            {
                byte a = (byte)(alpha * 255);
                var box = new BoxView { HeightRequest = ItemHeight, InputTransparent = true };
                box.SetAppThemeColor(BoxView.ColorProperty,
                    Color.FromRgba((byte)255, (byte)255, (byte)255, a),   // white fade for light mode
                    Color.FromRgba((byte)0, (byte)0, (byte)0, a));  // black fade for dark mode
                stack.Children.Add(box);
            }

            return stack;
        }
    }
}

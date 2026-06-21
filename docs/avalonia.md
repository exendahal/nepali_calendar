# NepaliDatePicker — Avalonia

A **Bikram Sambat (BS)** date picker for **Avalonia UI**, built on a Material Design calendar. Supports desktop (Windows, macOS, Linux) via a dialog window and mobile (Android, iOS) via a full-screen overlay. Includes a text-field-style input control, programmatic service API, Devanagari script rendering, and full color theming.

[![NuGet](https://img.shields.io/nuget/v/NepaliDatePicker.Avalonia)](https://www.nuget.org/packages/NepaliDatePicker.Avalonia/)

---

## Platform support

| Platform | Presentation |
|---|---|
| Windows / macOS / Linux | Centered dialog window (`ShowDialog`) |
| Android | Full-screen scrim overlay (`OverlayLayer`) |
| iOS | Full-screen scrim overlay (`OverlayLayer`) |

---

## Installation

```
dotnet add package NepaliDatePicker.Avalonia
```

Add the AXAML namespace where needed:

```xml
xmlns:npd="clr-namespace:NepaliDatePicker.Controls;assembly=NepaliDatePicker.Avalonia"
```

---

## `NepaliDatePickerField` — text-field input

A tappable date field that opens the picker on click/tap and fills with the selected date. Renders a placeholder when empty and adapts border color when a date is selected. Supports light and dark themes.

```xml
<npd:NepaliDatePickerField
    Label="Date of Birth"
    PlaceholderText="Select a BS date…"
    DateSelected="OnDateSelected" />
```

With theming:

```xml
<npd:NepaliDatePickerField
    Label="Payment Date"
    PlaceholderText="Select a BS date…"
    PickerOptions="{Binding TealOptions}"
    DateSelected="OnDateSelected" />
```

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `SelectedDate` | `NepaliDate?` | `null` | Currently selected date (bindable) |
| `PickerOptions` | `NepaliDatePickerOptions?` | `null` | Colors, style, script, labels |
| `PlaceholderText` | `string` | `"Select a date"` | Text shown when no date is selected |
| `Label` | `string?` | `null` | Optional label displayed above the field |

### Event

| Event | Signature | Fires when |
|---|---|---|
| `DateSelected` | `EventHandler<NepaliDate>` | User confirms a date in the picker |

---

## `NepaliDatePickerButton` — button trigger

A full-width button that opens the picker and shows the selected date as its text.

```xml
<npd:NepaliDatePickerButton
    ButtonText="Pick a date"
    PickerOptions="{Binding Options}"
    DateSelected="OnDateSelected" />
```

---

## Programmatic usage — `INepaliDatePickerService`

Use the service when you need to open the picker from code (e.g., on a gesture, from a ViewModel, or tied to a color swatch).

```csharp
// Constructor injection (or resolve from DI)
private readonly INepaliDatePickerService _picker = new NepaliDatePickerService();

private async void OnButtonClicked()
{
    var opts = new NepaliDatePickerOptions
    {
        UseNepaliScript       = true,
        PickerStyle           = PickerStyle.Calendar,
        PrimaryColor          = Color.Parse("#00695C"),
        HeaderBackgroundColor = Color.Parse("#00695C"),
    };

    NepaliDate? result = await _picker.ShowAsync(initialDate: null, options: opts);
    if (result is not null)
        SelectedDate = result;
}
```

From AXAML code-behind using a `Visual` anchor (avoids DI requirement):

```csharp
// On any event handler where 'this' is a Visual inside the window/activity:
var result = await NepaliPickerOverlay.ShowAsync(this, currentDate, options);
```

---

## `NepaliDatePickerOptions`

All color properties are nullable — unset values fall back to Material Design 3 defaults.

### Behaviour

| Property | Type | Default | Description |
|---|---|---|---|
| `UseNepaliScript` | `bool` | `false` | Render BS text in Devanagari script |
| `PickerStyle` | `PickerStyle` | `Calendar` | `Calendar` grid or `Input` (spinners) |
| `DisplayMode` | `DateDisplayMode` | `Both` | `BsOnly`, `AdOnly`, or `Both` (BS/AD toggle chip) |
| `SheetCornerRadius` | `double` | `12` | Corner radius of the picker card |
| `ShowHeader` | `bool` | `true` | Whether to display the colored header band |
| `CancelLabel` | `string` | `"CANCEL"` | Cancel button label |
| `ConfirmLabel` | `string` | `"OK"` | Confirm button label |
| `FontFamily` | `string?` | `null` | Custom font family for all picker text |

### Palette

| Property | Affects |
|---|---|
| `PrimaryColor` / `PrimaryColorDark` | Selected day fill, active chip, today ring, OK text |
| `OnPrimaryColor` | Text/icon drawn on top of primary-colored surface |
| `HeaderBackgroundColor` / `HeaderBackgroundColorDark` | Header band |
| `HeaderTextColor` | All text inside the header |
| `SurfaceColor` / `SurfaceColorDark` | Picker body background |
| `OnSurfaceColor` / `OnSurfaceColorDark` | Day numbers, month/year label |
| `OnSurfaceVariantColor` / `OnSurfaceVariantColorDark` | Weekday header initials |

---

## `PickerStyle` enum

| Value | Behaviour |
|---|---|
| `Calendar` *(default)* | Material Design month grid with year/month fast-nav |
| `Input` | Spinner-style numeric input (year / month / day) |

---

## Nepali script mode (`UseNepaliScript`)

When enabled:

- Month names: "Baisakh" → "बैशाख"
- Day numbers: 1–32 → १–३२
- Year: 2082 → २०८२
- Weekday headers: S M T W T F S → आइ सो मं बु बि शु श

---

## Theme-aware resources (App.axaml)

Add these to your `App.axaml` so card backgrounds adapt automatically in dark mode:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.ThemeDictionaries>
            <ResourceDictionary x:Key="Light">
                <SolidColorBrush x:Key="AppCardBackground"  Color="#FFFFFF" />
                <SolidColorBrush x:Key="AppCardBorder"       Color="#E5E0EB" />
                <SolidColorBrush x:Key="AppTintBackground"   Color="#F5F0FF" />
                <SolidColorBrush x:Key="AppSubtleTint"       Color="#F0F4FF" />
            </ResourceDictionary>
            <ResourceDictionary x:Key="Dark">
                <SolidColorBrush x:Key="AppCardBackground"  Color="#2B2930" />
                <SolidColorBrush x:Key="AppCardBorder"       Color="#47434E" />
                <SolidColorBrush x:Key="AppTintBackground"   Color="#2D2538" />
                <SolidColorBrush x:Key="AppSubtleTint"       Color="#252A38" />
            </ResourceDictionary>
        </ResourceDictionary.ThemeDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

Use them in AXAML with `{DynamicResource AppCardBackground}` etc.

---

## Safe area (Android / iOS)

Apply system bar insets in your main view so content doesn't overlap the status bar on Android 15+ (edge-to-edge is mandatory) or the home indicator on iOS:

```csharp
protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
{
    base.OnAttachedToVisualTree(e);
    var topLevel = TopLevel.GetTopLevel(this);
    if (topLevel?.InsetsManager is { } mgr)
    {
        void Apply(Thickness insets) =>
            RootScrollViewer.Padding = new Thickness(0, insets.Top, 0, insets.Bottom);
        Apply(mgr.SafeAreaPadding);
        mgr.SafeAreaChanged += (_, args) => Apply(args.SafeAreaPadding);
    }
}
```

---

## Project layout

```
src/avalonia/                          ← Avalonia NuGet library
│  NepaliDatePicker.Avalonia.csproj
│
├─ Controls/
│   NepaliDatePickerField.cs           text-field input control
│   NepaliDatePickerButton.cs          button trigger control
│   NepaliDatePickerView.cs            calendar / input picker UI
│   NepaliDatePickerWindow.cs          desktop dialog window
│   NepaliPickerOverlay.cs             shared overlay helper (desktop + mobile)
│
├─ Models/
│   NepaliDatePickerOptions.cs
│
└─ Services/
    INepaliDatePickerService.cs
    NepaliDatePickerService.cs

example/avalonia/                      ← demo Avalonia app
├─ Views/MainView.axaml                full-featured showcase
├─ desktop/                            Windows / macOS / Linux entry point
├─ android/                            Android activity
└─ ios/                                iOS app delegate
```

---

## License

MIT

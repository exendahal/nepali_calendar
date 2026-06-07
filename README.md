# NepaliDatePicker.Maui

A **Bikram Sambat (BS)** date picker for **.NET MAUI** built on a Material Design 3 calendar grid. Supports bottom-sheet and dialog presentation, BS ↔ AD dual-calendar toggle, full Devanagari script rendering, and rich theming.

---

## Features

| Feature | Details |
|---|---|
| **Material Design 3 calendar** | Month grid with today highlight, selected-day fill, and smooth navigation |
| **Dual presentation** | Bottom sheet (slide-up) or centered dialog (fade + scale) |
| **BS / AD toggle** | Chip switch lets users pick in Bikram Sambat or Gregorian; live header updates |
| **Devanagari script** | Month names, day/year numerals, weekday labels all render in Nepali script |
| **Light & dark theme** | All surfaces and text use `SetAppThemeColor`; follows system appearance |
| **Year / month fast-nav** | Tap the month–year label to open a scrollable year + month grid |
| **Auto day clamping** | BS months vary 29–32 days; changing year or month re-clamps the selected day |
| **BS year range** | 1970 – 2100 BS (official Government of Nepal calendar data) |
| **Drop-in XAML control** | `<nep:NepaliDatePicker>` — bind `SelectedDate`, done |
| **DI / MVVM service** | `INepaliDatePickerService` registered via `AddNepaliDatePicker()` |
| **Full theming** | Primary color, header, surface, text — every MD3 token is overridable |

---

## Installation

```csharp
// MauiProgram.cs
builder.AddNepaliDatePicker();
```

Add the XAML namespace where needed:

```xml
xmlns:nep="clr-namespace:NepaliDatePicker.Controls;assembly=NepaliDatePicker.Maui"
```

---

## Usage

### Drop-in XAML control

```xml
<nep:NepaliDatePicker
    SelectedDate="{Binding MyDate}"
    Placeholder="Pick a date…"
    Format="d MMMM yyyy"
    DisplayMode="Both"
    UseNepaliScript="False"
    DateSelected="OnDateSelected" />
```

### Programmatic / MVVM

```csharp
// Inject INepaliDatePickerService
public MyViewModel(INepaliDatePickerService picker) => _picker = picker;

[RelayCommand]
async Task OpenPicker()
{
    var opts = new NepaliDatePickerOptions
    {
        DisplayMode     = DateDisplayMode.Both,
        Presentation    = PickerPresentation.BottomSheet,
        UseNepaliScript = false,
    };
    NepaliDate? result = await _picker.ShowAsync(SelectedDate, opts);
    if (result is not null)
        SelectedDate = result;
}
```

---

## `NepaliDatePicker` control

The drop-in control renders as a tappable date-input field. All properties are bindable.

### Core

| Property | Type | Default | Description |
|---|---|---|---|
| `SelectedDate` | `NepaliDate?` | `null` | Two-way bindable selected date |
| `Placeholder` | `string` | `"Select Date"` | Text shown when no date is selected |
| `Format` | `string` | `"d MMMM yyyy"` | Format tokens: `d` `dd` `M` `MM` `MMMM` `yy` `yyyy` |
| `IsReadOnly` | `bool` | `false` | Prevents the picker from opening |

### Picker behaviour

| Property | Type | Default | Description |
|---|---|---|---|
| `DisplayMode` | `DateDisplayMode` | `Both` | Which calendar system(s) the picker exposes |
| `Presentation` | `PickerPresentation` | `BottomSheet` | Bottom sheet or centered dialog |
| `UseNepaliScript` | `bool` | `false` | Render BS dates in Devanagari script |

### Picker theming (passed through to the sheet)

| Property | Type | Default | Description |
|---|---|---|---|
| `PrimaryColor` | `Color?` | MD3 purple | Selected day fill, active chip, today outline |
| `PrimaryColorDark` | `Color?` | `PrimaryColor` | Dark-theme override |
| `PickerHeaderColor` | `Color?` | MD3 purple | Header band background color |
| `PickerFontFamily` | `string?` | `null` | Custom font for all text inside the picker |

### Button appearance

| Property | Type | Default | Description |
|---|---|---|---|
| `BorderColor` | `Color` | `#C8C8C8` | Input field border (light theme) |
| `BorderColorDark` | `Color` | `#3A3A3A` | Input field border (dark theme) |
| `InputBackgroundColor` | `Color` | `#F2F2F7` | Input field fill (light theme) |
| `InputBackgroundColorDark` | `Color` | `#2C2C2E` | Input field fill (dark theme) |
| `TextColor` | `Color` | `#111111` | Date text color (light theme) |
| `TextColorDark` | `Color` | `#EEEEEE` | Date text color (dark theme) |
| `PlaceholderColor` | `Color` | `#AAAAAA` | Placeholder text color (light theme) |
| `PlaceholderColorDark` | `Color` | `#666666` | Placeholder text color (dark theme) |
| `FontFamily` | `string?` | `null` | Font for the date / placeholder label |
| `FontSize` | `double` | `15.0` | Font size of the date / placeholder label |
| `CornerRadius` | `double` | `10.0` | Corner radius of the input field border |
| `CalendarIcon` | `string` | `"📅"` | Icon shown on the right side of the field |

### Event

| Event | Signature | Fires when |
|---|---|---|
| `DateSelected` | `EventHandler<NepaliDate?>` | User confirms a date in the picker |

---

## `NepaliDatePickerOptions`

Passed to `INepaliDatePickerService.ShowAsync()` for full programmatic control.

### Behaviour

| Property | Type | Default | Description |
|---|---|---|---|
| `DisplayMode` | `DateDisplayMode` | `Both` | `BsOnly`, `AdOnly`, or `Both` (shows BS/AD chip toggle) |
| `Presentation` | `PickerPresentation` | `BottomSheet` | `BottomSheet` or `Dialog` |
| `UseNepaliScript` | `bool` | `false` | All BS text rendered in Devanagari script |
| `SheetCornerRadius` | `double` | `28` | Top-corner radius of the bottom sheet / dialog |
| `FontFamily` | `string?` | `null` | Custom font family for all picker labels |

### Full palette

Every color token is nullable — unset tokens fall back to MD3 defaults.

| Property | Affects |
|---|---|
| `PrimaryColor` / `PrimaryColorDark` | Selected day fill, active chip background, today ring, OK button text |
| `OnPrimaryColor` | Text/icon drawn on top of the primary-filled circle or chip |
| `HeaderBackgroundColor` / `HeaderBackgroundColorDark` | Colored header band |
| `HeaderTextColor` | All text inside the header band |
| `SurfaceColor` / `SurfaceColorDark` | Picker body background |
| `OnSurfaceColor` / `OnSurfaceColorDark` | Day numbers, month/year label |
| `OnSurfaceVariantColor` / `OnSurfaceVariantColorDark` | Day-of-week header initials |

---

## `DateDisplayMode` enum

| Value | Behaviour |
|---|---|
| `Both` *(default)* | Picker shows a BS / AD chip toggle; user can switch between calendars |
| `BsOnly` | Only the Bikram Sambat calendar is available; chip toggle is hidden |
| `AdOnly` | Only the Gregorian calendar is available; chip toggle is hidden |

---

## `PickerPresentation` enum

| Value | Behaviour |
|---|---|
| `BottomSheet` *(default)* | Sheet slides up from the bottom with a cubic-ease animation |
| `Dialog` | Sheet floats centered over the page with a fade + scale animation |

---

## Nepali script mode (`UseNepaliScript`)

When enabled (and `DisplayMode` is `BsOnly` or `Both` with BS active):

- **Month names** — "Baisakh" → "बैशाख", "Jestha" → "जेठ", … "Chaitra" → "चैत्र"
- **Day numbers** — 1–32 → १–३२
- **Year number** — 2082 → २०८२
- **Weekday header row** — S M T W T F S → आइ सो मं बु बि शु श
- **Header date line** — "Sun, Baisakh 5, 2082" → "आइत, बैशाख ५, २०८२"
- **Header label** — "SELECT DATE" → "मिति छान्नुहोस्"

AD dates always remain in English regardless of this setting.

---

## `INepaliDatePickerService`

```csharp
public interface INepaliDatePickerService
{
    /// Current date in Bikram Sambat.
    NepaliDate Today { get; }

    /// Open the picker and await the user's selection.
    /// Returns null if the user cancels.
    Task<NepaliDate?> ShowAsync(NepaliDate? initialDate = null,
                                NepaliDatePickerOptions? options = null);
}
```

Registered as **transient** by `AddNepaliDatePicker()`. Inject via constructor in your ViewModel.

---

## `NepaliDate` model

```csharp
var d = new NepaliDate(2082, 1, 15);

d.Year            // 2082
d.Month           // 1
d.Day             // 15
d.MonthName       // "Baisakh"
d.MonthNameNepali // "बैशाख"
d.ToDisplayString() // "15 Baisakh 2082"
d.ToString()        // "2082/01/15"
d.IsValid()         // true
```

---

## BS ↔ AD conversion

```csharp
using NepaliDatePicker.Services;

// BS → AD
DateTime ad = BsAdConverter.BsToAd(new NepaliDate(2082, 1, 15));

// AD → BS
NepaliDate bs = BsAdConverter.AdToBs(DateTime.Today);

// Formatted AD string
string label = BsAdConverter.FormatAdEquivalent(new NepaliDate(2082, 1, 15));
// → "28 April 2025"
```

Conversion data covers **1970 – 2100 BS** using official Government of Nepal calendar tables.

---

## Project layout

```
src/                               ← NuGet library
│  MauiAppBuilderExtensions.cs     service registration
│  NepaliDatePickerPage.cs         modal page (scrim + sheet)
│  NepaliDatePickerService.cs      INepaliDatePickerService implementation
│
├─ Controls/
│   NepaliDatePickerSheet.cs       MD3 calendar sheet (header, nav, grid, year picker)
│   NepaliDatePickerButton.cs      drop-in bindable input control
│   DrumRollPicker.cs              scroll-snap drum roll (internal)
│
├─ Data/
│   BsCalendarData.cs              1970–2100 BS month-day counts
│
├─ Models/
│   NepaliDate.cs                  record: Year / Month / Day + helpers
│   NepaliDatePickerOptions.cs     picker configuration
│   DateDisplayMode.cs             BsOnly / AdOnly / Both enum
│   PickerPresentation.cs          BottomSheet / Dialog enum
│
└─ Services/
    BsAdConverter.cs               BS ↔ AD conversion utilities
    INepaliDatePickerService.cs    public service interface

example/                           ← demo MAUI app (Android / iOS / Windows)
```

---

## License

MIT

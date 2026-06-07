# NepaliDatePicker.Maui

A Nepali **Bikram Sambat (BS)** date picker for **.NET MAUI** with a native bottom-sheet UI.

## Features

| Feature | Details |
|---|---|
| **Bottom sheet** | Slides up with a cubic-ease animation; scrim tap or Cancel dismisses it |
| **Drum-roll pickers** | Year · Month · Day — scroll to select, auto-snaps to nearest item |
| **Auto day clamping** | Changing year or month recalculates valid days (BS months vary 29–32 days) |
| **AD reference** | Live "AD: dd MMMM yyyy" label updates while you scroll, so users can cross-check |
| **BS / AD toggle** | Tab switcher at top of sheet — flip between picking in BS or AD |
| **Light & dark mode** | Uses `SetAppThemeColor` for all surfaces; follows system appearance |
| **BS year range** | 1970 – 2100 BS (official Government of Nepal calendar data) |
| **Drop-in control** | `NepaliDatePicker` — bind `SelectedDate`, done |
| **DI / MVVM** | `INepaliDatePickerService` registered via `AddNepaliDatePicker()` |

---

## Quick start

### 1. Register

```csharp
// MauiProgram.cs
builder.AddNepaliDatePicker();
```

### 2a — Drop-in XAML control

```xml
xmlns:nep="clr-namespace:NepaliDatePicker.Controls;assembly=NepaliDatePicker.Maui"

<nep:NepaliDatePicker
    SelectedDate="{Binding MyBsDate}"
    Placeholder="Pick a date…"
    Format="d MMMM yyyy"
    DateSelected="OnDateSelected" />
```

### 2b — Programmatic (MVVM)

```csharp
// Constructor-inject INepaliDatePickerService
public MyViewModel(INepaliDatePickerService picker)
{
    _picker = picker;
}

[RelayCommand]
async Task OpenPicker()
{
    NepaliDate? result = await _picker.ShowAsync(currentDate);
    if (result is not null)
        SelectedDate = result;
}
```

---

## BS ↔ AD conversion

```csharp
using NepaliDatePicker.Services;

DateTime ad = BsAdConverter.BsToAd(new NepaliDate(2081, 2, 15));
NepaliDate bs = BsAdConverter.AdToBs(DateTime.Today);
string label = BsAdConverter.FormatAdEquivalent(new NepaliDate(2081, 6, 1)); // "17 September 2024"
```

---

## `NepaliDate` model

```csharp
var d = new NepaliDate(2081, 6, 1);
d.MonthName          // "Ashwin"
d.MonthNameNepali    // "आश्विन"
d.ToDisplayString()  // "1 Ashwin 2081"
d.ToString()         // "2081/06/01"
d.IsValid()          // true
```

---

## Project layout

```
NepaliDatePicker/              ← NuGet library
│  MauiAppBuilderExtensions.cs
│  NepaliDatePickerPage.cs     (modal page hosting the sheet)
│  NepaliDatePickerService.cs
│
├─ Controls/
│   DrumRollPicker.cs          (scroll-snap drum roll)
│   NepaliDatePickerSheet.cs   (sheet content: header + tabs + pickers)
│   NepaliDatePicker.cs  (drop-in bindable button control)
│
├─ Data/
│   BsCalendarData.cs          (1970–2100 month-day counts)
│
├─ Models/
│   NepaliDate.cs
│
└─ Services/
    BsAdConverter.cs
    INepaliDatePickerService.cs

NepaliDatePickerDemo/          ← demo MAUI app
```

---

## License

MIT

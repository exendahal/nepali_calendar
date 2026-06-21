# NepaliDatePicker

A **Bikram Sambat (BS)** date picker library for .NET — available for both **.NET MAUI** and **Avalonia UI**. Supports calendar and input picker styles, BS ↔ AD dual-calendar toggle, full Devanagari script rendering, light/dark theme, and comprehensive color theming.

---

## Choose your platform

| | MAUI | Avalonia |
|---|---|---|
| **NuGet** | [![NepaliDatePicker.Maui](https://img.shields.io/nuget/v/NepaliDatePicker.Maui)](https://www.nuget.org/packages/NepaliDatePicker.Maui/) | [![NepaliDatePicker.Avalonia](https://img.shields.io/nuget/v/NepaliDatePicker.Avalonia)](https://www.nuget.org/packages/NepaliDatePicker.Avalonia/) |
| **Targets** | Android, iOS, Windows, macOS | Windows, macOS, Linux, Android, iOS |
| **Docs** | [docs/maui.md](docs/maui.md) | [docs/avalonia.md](docs/avalonia.md) |

---

## Features

| Feature | Details |
|---|---|
| **Material Design calendar** | Month grid with today highlight, selected-day fill, and smooth navigation |
| **BS / AD toggle** | Chip switch lets users pick Bikram Sambat or Gregorian; live header updates |
| **Devanagari script** | Month names, day/year numerals, weekday labels all render in Nepali script |
| **Light & dark theme** | All surfaces and text adapt to system appearance automatically |
| **Year / month fast-nav** | Tap the month–year label to open a scrollable year + month grid |
| **Auto day clamping** | BS months vary 29–32 days; changing year or month re-clamps the selected day |
| **BS year range** | 1970 – 2100 BS (official Government of Nepal calendar data) |
| **Drop-in control** | Bind `SelectedDate`, done |
| **Service API** | `INepaliDatePickerService` for programmatic / MVVM usage |
| **Full theming** | Primary color, header, surface, text — every MD3 token overridable |

---

## Common models

Both packages share the same core models.

### `NepaliDate`

```csharp
var d = new NepaliDate(2082, 1, 15);

d.Year              // 2082
d.Month             // 1
d.Day               // 15
d.MonthName         // "Baisakh"
d.MonthNameNepali   // "बैशाख"
d.ToDisplayString() // "15 Baisakh 2082"
d.ToString()        // "2082/01/15"
d.IsValid()         // true
```

### `BsAdConverter`

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

## Date utilities

Add `using NepaliDatePicker.Formatting;` to access formatting helpers.

### `NepaliDateFormatter` — token-based formatting

```csharp
using NepaliDatePicker.Formatting;

var date = new NepaliDate(2082, 1, 15);

NepaliDateFormatter.Format(date, "d MMMM yyyy")               // "15 Baisakh 2082"
NepaliDateFormatter.Format(date, "dd/MM/yyyy")                // "15/01/2082"
NepaliDateFormatter.Format(date, "EEEE, d MMMM yyyy")         // "Tuesday, 15 Baisakh 2082"
NepaliDateFormatter.Format(date, "d MMMM yyyy", nepali: true) // "१५ बैशाख २०८२"

// Extension method
date.Format("d MMMM yyyy")
```

#### Pattern tokens

| Token | Output |
|---|---|
| `yyyy` | 4-digit year — `2082` |
| `yy` | 2-digit year — `82` |
| `MMMM` | Full month name — `Baisakh` / `बैशाख` |
| `MMM` | Abbreviated month — `Bai` |
| `MM` | Zero-padded month — `01` |
| `M` | Month number — `1` |
| `dd` | Zero-padded day — `05` |
| `d` | Day number — `5` |
| `EEEE` | Full weekday — `Tuesday` / `मंगलबार` |
| `EEE` | Short weekday — `Tue` / `मंगल` |
| `EE` | Minimal weekday — `Tu` / `मं` |

Wrap literal characters in single quotes — `'of'` — to prevent token substitution.

---

### `NepaliMoment` — relative time

```csharp
using NepaliDatePicker.Formatting;

var date = new NepaliDate(2082, 1, 12);

NepaliMoment.Elapsed(date)                  // "3 days ago"
NepaliMoment.Elapsed(date, nepali: true)    // "३ दिन पहिले"

// From AD DateTime
NepaliMoment.Elapsed(DateTime.Today.AddDays(-8))  // "1 week ago"

// Extension method
date.Elapsed()
date.Elapsed(nepali: true)
```

#### Threshold behaviour

| Difference | English | Nepali |
|---|---|---|
| Under 1 minute | `Just now` | `भर्खरै` |
| Under 1 hour | `X minutes ago / In X minutes` | `X मिनेट पहिले` |
| Under 1 day | `X hours ago / In X hours` | `X घण्टा पहिले` |
| Exactly 1 day | `Yesterday / Tomorrow` | `हिजो / भोलि` |
| Under 7 days | `X days ago / In X days` | `X दिन पहिले` |
| Under 30 days | `X weeks ago / In X weeks` | `X हप्ता पहिले` |
| Under 365 days | `X months ago / In X months` | `X महिना पहिले` |
| 365 days+ | `X years ago / In X years` | `X वर्ष पहिले` |

---

## Repository layout

```
src/
├─ core/            shared models, conversion, formatting utilities
├─ maui/            NepaliDatePicker.Maui library
└─ avalonia/        NepaliDatePicker.Avalonia library

example/
├─ maui/            MAUI demo app (Android / iOS / Windows)
└─ avalonia/        Avalonia demo app (Desktop / Android / iOS)

docs/
├─ maui.md          MAUI platform guide
└─ avalonia.md      Avalonia platform guide
```

---

## License

MIT

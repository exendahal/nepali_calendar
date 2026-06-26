# Nepali Calendar

A collection of .NET libraries for the **Bikram Sambat (BS / Nepali)** calendar system.

| Package | NuGet | License | Description |
|---|---|---|---|
| **NepaliUtility.Core** | [![NuGet](https://img.shields.io/nuget/v/NepaliUtility.Core)](https://www.nuget.org/packages/NepaliUtility.Core/) | [![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT) | Utility library for Nepali .NET apps — BS calendar, formatting, and more |
| **NepaliDatePicker.Maui** | [![NuGet](https://img.shields.io/nuget/v/NepaliDatePicker.Maui)](https://www.nuget.org/packages/NepaliDatePicker.Maui/) | [![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT) | BS date picker control for .NET MAUI |
| **NepaliDatePicker.Avalonia** | [![NuGet](https://img.shields.io/nuget/v/NepaliDatePicker.Avalonia)](https://www.nuget.org/packages/NepaliDatePicker.Avalonia/) | [![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT) | BS date picker control for Avalonia UI |

---

## NepaliUtility.Core

A zero-dependency utility library for .NET developers building Nepali applications. Works in any .NET 8+ project — console, ASP.NET Core, Blazor, MAUI, Avalonia — no UI framework required.

**Features**

- BS ↔ AD date conversion (`BsAdConverter`) with `DateTime` and `DateOnly` support
- Authoritative Government of Nepal calendar data for **BS 1970–2100**
- Immutable `NepaliDate` record — arithmetic, comparison operators, parsing, validation
- Token-based date formatting (`"EEEE, d MMMM yyyy"`) with full Devanagari output
- Relative-time strings in English and Nepali (`"3 days ago"` / `"३ दिन पहिले"`)
- Devanagari numeral conversion (`NepaliNumberConverter`)
- Nepal fiscal year helpers (`NepaliFiscalYear`)

**Install**

```
dotnet add package NepaliUtility.Core
```

**Quick start**

```csharp
using NepaliUtility.Services;
using NepaliUtility.Formatting;

NepaliDate today = BsAdConverter.Today;              // today in BS
DateTime   ad    = BsAdConverter.BsToAd(today);      // BS → AD

today.Format("EEEE, d MMMM yyyy")                    // "Thursday, 15 Baisakh 2082"
today.Format("d MMMM yyyy", nepali: true)             // "१५ बैशाख २०८२"
today.Elapsed()                                       // "Just now"
```

**Full API reference → [src/core/README.md](src/core/README.md)**

---

## NepaliDatePicker.Maui

A Material Design BS date picker for **.NET MAUI** — available as a drop-in XAML control or a DI service. Runs on Android, iOS, Windows, and macOS.

**Features**

- Material Design 3 calendar grid and iOS-style wheel picker
- Bottom-sheet or dialog presentation
- BS / AD dual-calendar chip toggle
- Full Devanagari script rendering
- Light & dark theme (follows system appearance)
- Year / month fast-nav grid
- Auto day clamping for variable BS month lengths
- Full MD3 color palette override
- `INepaliDatePickerService` for programmatic / MVVM usage

**Install**

```
dotnet add package NepaliDatePicker.Maui
```

```csharp
// MauiProgram.cs
builder.AddNepaliDatePicker();
```

**Quick start**

```xml
xmlns:nep="clr-namespace:NepaliDatePicker.Controls;assembly=NepaliDatePicker.Maui"

<nep:NepaliDatePicker
    SelectedDate="{Binding MyDate}"
    Placeholder="Pick a date…"
    Format="d MMMM yyyy"
    DisplayMode="Both"
    DateSelected="OnDateSelected" />
```

**Full documentation → [docs/maui.md](docs/maui.md)**

---

## NepaliDatePicker.Avalonia

A Material Design BS date picker for **Avalonia UI** — text-field input control, standalone button trigger, and a programmatic service API. Runs on Windows, macOS, Linux, Android, and iOS.

**Platform behaviour**

| Platform | Presentation |
|---|---|
| Windows / macOS / Linux | Centered dialog window |
| Android / iOS | Full-screen overlay |

**Features**

- `NepaliDatePickerField` — tappable text field with placeholder and label
- `NepaliDatePickerButton` — button trigger that shows the selected date
- Calendar grid and spinner-style input picker styles
- BS / AD dual-calendar chip toggle
- Full Devanagari script rendering
- Light & dark theme
- Full MD3 color palette override
- `INepaliDatePickerService` for programmatic usage

**Install**

```
dotnet add package NepaliDatePicker.Avalonia
```

**Quick start**

```xml
xmlns:npd="clr-namespace:NepaliDatePicker.Controls;assembly=NepaliDatePicker.Avalonia"

<npd:NepaliDatePickerField
    Label="Date of Birth"
    PlaceholderText="Select a BS date…"
    DateSelected="OnDateSelected" />
```

**Full documentation → [docs/avalonia.md](docs/avalonia.md)**

---

## Repository layout

```
src/
├─ core/        NepaliUtility.Core          — BS calendar primitives (NuGet)
├─ maui/        NepaliDatePicker.Maui       — MAUI picker control (NuGet)
└─ avalonia/    NepaliDatePicker.Avalonia   — Avalonia picker control (NuGet)

example/
├─ maui/        MAUI demo app (Android / iOS / Windows / macOS)
└─ avalonia/    Avalonia demo app (Desktop / Android / iOS)

docs/
├─ maui.md      Full MAUI API reference
└─ avalonia.md  Full Avalonia API reference
```

---

## License

MIT

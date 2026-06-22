# NepaliUtility.Core

A zero-dependency .NET library for the **Bikram Sambat (BS/Nepali)** calendar system.

[![NuGet](https://img.shields.io/nuget/v/NepaliUtility.Core)](https://www.nuget.org/packages/NepaliUtility.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)

---

## Installation

```
dotnet add package NepaliUtility.Core
```

Targets **.NET 8.0+**. No UI framework dependencies — works in console apps, ASP.NET Core, MAUI, Avalonia, Blazor, and any other .NET project.

---

## Namespaces

| Namespace | Contents |
|---|---|
| `NepaliUtility.Models` | `NepaliDate`, `DateDisplayMode`, `PickerPresentation`, `PickerStyle`, `NepaliFiscalYear` |
| `NepaliUtility.Data` | `BsCalendarData` |
| `NepaliUtility.Services` | `BsAdConverter`, `NepaliFiscalYear` |
| `NepaliUtility.Formatting` | `NepaliDateFormatter`, `NepaliMoment`, `NepaliNumberConverter`, extension methods |

---

## NepaliDate

`NepaliDate` is an immutable record representing a date in the Bikram Sambat calendar.

```csharp
using NepaliUtility.Models;

var d = new NepaliDate(2082, 1, 15);

d.Year              // 2082
d.Month             // 1
d.Day               // 15
d.MonthName         // "Baisakh"
d.MonthNameNepali   // "बैशाख"
d.ToString()        // "2082/01/15"
d.ToDisplayString() // "15 Baisakh 2082"
d.IsValid()         // true
```

### Date arithmetic

```csharp
d.AddDays(10)    // NepaliDate(2082, 1, 25) — extension, using NepaliUtility.Formatting
d.AddMonths(3)   // NepaliDate(2082, 4, 15)
d.AddYears(1)    // NepaliDate(2083, 1, 15)
```

### Day of week

```csharp
using NepaliUtility.Formatting;

d.GetDayOfWeek()  // DayOfWeek.Tuesday
```

### Days between dates

```csharp
var a = new NepaliDate(2082, 1, 1);
var b = new NepaliDate(2082, 2, 1);
a.DaysTo(b)  // 31
```

### Parsing

```csharp
var d = NepaliDate.Parse("2082/01/15");   // or "2082-01-15"
NepaliDate.TryParse("2082/01/15", out var result);  // true
```

### Comparison

`NepaliDate` implements `IComparable<NepaliDate>` and the `<`, `>`, `<=`, `>=` operators.

```csharp
var a = new NepaliDate(2082, 1, 1);
var b = new NepaliDate(2082, 6, 1);

a < b   // true
a > b   // false
a.CompareTo(b)  // negative
```

---

## BsAdConverter

Convert between Bikram Sambat and Gregorian (AD) dates. Coverage: **BS 1970–2100** using official Government of Nepal calendar data.

```csharp
using NepaliUtility.Services;

// Today in BS
NepaliDate today = BsAdConverter.Today;

// BS → AD
DateTime ad = BsAdConverter.BsToAd(new NepaliDate(2082, 1, 15));
// → DateTime (2025-04-28)

// AD → BS
NepaliDate bs = BsAdConverter.AdToBs(DateTime.Today);

// DateOnly support (.NET 8+)
DateOnly adOnly = BsAdConverter.BsToAdDateOnly(new NepaliDate(2082, 1, 15));
NepaliDate bs2  = BsAdConverter.AdToBs(DateOnly.FromDateTime(DateTime.Today));

// Formatted display strings
string adLabel = BsAdConverter.FormatAdEquivalent(new NepaliDate(2082, 1, 15));
// → "28 April 2025"

string bsLabel = BsAdConverter.FormatBsEquivalent(DateTime.Today);
// → "15 Baisakh 2082"
```

---

## BsCalendarData

Raw calendar data and helper methods.

```csharp
using NepaliUtility.Data;

BsCalendarData.MinYear  // 1970
BsCalendarData.MaxYear  // 2100

BsCalendarData.GetDaysInMonth(2082, 1)   // 31
BsCalendarData.GetDaysInYear(2082)       // 365
BsCalendarData.GetMonthDays(2082)        // int[12]
BsCalendarData.IsYearSupported(2082)     // true
BsCalendarData.ClampDay(2082, 1, 99)     // 31
```

---

## NepaliDateFormatter

Token-based date formatting, inspired by Java's DateTimeFormatter.

```csharp
using NepaliUtility.Formatting;

var date = new NepaliDate(2082, 1, 15);

NepaliDateFormatter.Format(date, "d MMMM yyyy")               // "15 Baisakh 2082"
NepaliDateFormatter.Format(date, "dd/MM/yyyy")                // "15/01/2082"
NepaliDateFormatter.Format(date, "EEEE, d MMMM yyyy")         // "Tuesday, 15 Baisakh 2082"
NepaliDateFormatter.Format(date, "d MMMM yyyy", nepali: true) // "१५ बैशाख २०८२"

// Extension method
date.Format("EEEE, d MMMM yyyy")
date.Format("d MMMM yyyy", nepali: true)
```

### Format tokens

| Token | Output |
|---|---|
| `yyyy` | 4-digit BS year — `2082` |
| `yy` | 2-digit BS year — `82` |
| `MMMM` | Full month name — `Baisakh` / `बैशाख` |
| `MMM` | Abbreviated month — `Bai` |
| `MM` | Zero-padded month — `01` |
| `M` | Month number — `1` |
| `dd` | Zero-padded day — `05` |
| `d` | Day number — `5` |
| `EEEE` | Full weekday — `Tuesday` / `मंगलबार` |
| `EEE` | Short weekday — `Tue` / `मंगल` |
| `EE` | Minimal weekday — `Tu` / `मं` |

Wrap literal text in single quotes: `"'Miti:' d MMMM yyyy"` → `"Miti: 15 Baisakh 2082"`.

---

## NepaliMoment — relative time

```csharp
using NepaliUtility.Formatting;

var date = new NepaliDate(2082, 1, 12);

NepaliMoment.Elapsed(date)               // "3 days ago"
NepaliMoment.Elapsed(date, nepali: true) // "३ दिन पहिले"

// From AD DateTime
NepaliMoment.Elapsed(DateTime.Today.AddDays(-8))  // "1 week ago"

// Extension methods
date.Elapsed()
date.Elapsed(nepali: true)
```

### Thresholds

| Range | English | Nepali |
|---|---|---|
| < 1 min | `Just now` | `भर्खरै` |
| < 1 hr | `X minutes ago` | `X मिनेट पहिले` |
| < 1 day | `X hours ago` | `X घण्टा पहिले` |
| 1 day | `Yesterday / Tomorrow` | `हिजो / भोलि` |
| < 7 days | `X days ago` | `X दिन पहिले` |
| < 30 days | `X weeks ago` | `X हप्ता पहिले` |
| < 365 days | `X months ago` | `X महिना पहिले` |
| 365+ days | `X years ago` | `X वर्ष पहिले` |

---

## NepaliNumberConverter — Devanagari numerals

```csharp
using NepaliUtility.Formatting;

NepaliNumberConverter.ToDevanagari(2082)       // "२०८२"
NepaliNumberConverter.ToDevanagari("2082/01/15") // "२०८२/०१/१५"
NepaliNumberConverter.ToEnglish("२०८२")        // "2082"
```

---

## NepaliFiscalYear

Nepal's fiscal year runs from **1 Shrawan (month 4)** to the **last day of Ashad (month 3)** of the following year.

```csharp
using NepaliUtility.Services;

var date = new NepaliDate(2082, 5, 10); // Bhadra 10, 2082

NepaliFiscalYear.GetFiscalYear(date)          // 2082
NepaliFiscalYear.GetFiscalYearString(date)    // "2082/83"
NepaliFiscalYear.GetFiscalYearStartDate(2082) // NepaliDate(2082, 4, 1)  — 1 Shrawan 2082
NepaliFiscalYear.GetFiscalYearEndDate(2082)   // NepaliDate(2083, 3, 32) — last day of Ashad 2083
NepaliFiscalYear.IsSameFiscalYear(
    new NepaliDate(2082, 4, 1),
    new NepaliDate(2083, 3, 1))               // true
```

---

## Month & weekday name tables

```csharp
using NepaliUtility.Models;

NepaliDate.MonthNames        // ["Baisakh", "Jestha", ..., "Chaitra"]
NepaliDate.MonthNamesNepali  // ["बैशाख", "जेठ", ..., "चैत्र"]
NepaliDate.AdMonthNamesNepali // ["जनवरी", ..., "डिसेम्बर"]
```

---

## License

MIT © Santosh Dahal

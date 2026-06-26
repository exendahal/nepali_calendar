# NepaliUtility.Core

A zero-dependency utility library for .NET developers building Nepali applications. Focused on the **Bikram Sambat (BS / Nepali)** calendar system.

[![NuGet](https://img.shields.io/nuget/v/NepaliUtility.Core)](https://www.nuget.org/packages/NepaliUtility.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)

---

## Features

- **Current BS date & time** — `BsAdConverter.Now` and `BsAdConverter.Today` using Nepal Standard Time (UTC+5:45)
- **BS ↔ AD conversion** — convert between Bikram Sambat and Gregorian dates with `DateTime` and `DateOnly` support
- **`NepaliDateTime`** — combined BS date + time-of-day record with `Parse` / `TryParse`
- **Nepal timezone** — `NepaliTimeZone` with UTC+5:45 offset and conversion helpers
- **Authoritative calendar data** — Government of Nepal day-count tables covering BS 1970–2100
- **Immutable `NepaliDate` record** — arithmetic (`AddDays`, `AddMonths`, `AddYears`), comparison operators, parsing, and validation
- **Token-based date/time formatting** — `NepaliDateFormatter` with 30+ tokens including time, era, and quarter
- **Predefined format shortcuts** — `NepaliDateFormat.MEd()`, `NepaliDateFormat.jms()`, and 33 others
- **Relative-time strings** — `NepaliMoment` produces "3 days ago" / "३ दिन पहिले"
- **Number formatting** — `NepaliNumberFormat` with Nepali comma grouping, currency symbol, and in-words conversion (English and Nepali denominations)
- **Devanagari numeral conversion** — `NepaliNumberConverter` converts digits to/from Nepali script (०–९)
- **Nepal fiscal year helpers** — `NepaliFiscalYear` with string, start/end dates, and same-FY checks
- **Zero dependencies** — works in console, ASP.NET Core, Blazor, MAUI, Avalonia, and any other .NET 8+ project

---

## Installation

```
dotnet add package NepaliUtility.Core
```

Targets **.NET 8.0+**.

---

## Namespaces

| Namespace | Contents |
|---|---|
| `NepaliUtility.Models` | `NepaliDate`, `NepaliDateTime`, `DateDisplayMode`, `PickerPresentation`, `PickerStyle` |
| `NepaliUtility.Services` | `BsAdConverter`, `NepaliFiscalYear`, `NepaliTimeZone` |
| `NepaliUtility.Formatting` | `NepaliDateFormatter`, `NepaliDateFormat`, `NepaliMoment`, `NepaliNumberFormat`, `NepaliNumberConverter`, extension methods |
| `NepaliUtility.Data` | `BsCalendarData` |

---

## Current date & time

```csharp
using NepaliUtility.Services;

NepaliDateTime now   = BsAdConverter.Now;    // current BS date + Nepal time
NepaliDate     today = BsAdConverter.Today;  // current BS date only

DateTimeOffset nepalNow = NepaliTimeZone.Now;            // current AD time in UTC+5:45
DateTimeOffset converted = NepaliTimeZone.ToNepalTime(utcDateTime); // convert any UTC to NST
```

---

## NepaliDate

An immutable record representing a Bikram Sambat date.

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

### Arithmetic

```csharp
using NepaliUtility.Formatting;

d.AddDays(10)    // NepaliDate(2082, 1, 25)
d.AddMonths(3)   // NepaliDate(2082, 4, 15)
d.AddYears(1)    // NepaliDate(2083, 1, 15)
d.GetDayOfWeek() // DayOfWeek.Tuesday
d.DaysTo(other)  // int — positive if other is in the future
```

### Parsing & comparison

```csharp
NepaliDate.Parse("2082/01/15");             // accepts "/" or "-" separator
NepaliDate.TryParse("2082-01-15", out var r);

var a = new NepaliDate(2082, 1, 1);
var b = new NepaliDate(2082, 6, 1);
a < b            // true
a.CompareTo(b)   // negative
```

---

## NepaliDateTime

A combined BS date + time-of-day record.

```csharp
using NepaliUtility.Models;
using NepaliUtility.Services;

NepaliDateTime now = BsAdConverter.Now;

now.Year        // 2082
now.Hour        // 14
now.Minute      // 30
now.ToString()  // "2082/01/15 14:30:00"

// Parsing
NepaliDateTime.Parse("2082/01/15 21:17:56");
NepaliDateTime.TryParse("2082/01/15 09:04", out var result);
```

---

## BsAdConverter

```csharp
using NepaliUtility.Services;

NepaliDateTime now   = BsAdConverter.Now;            // BS date + Nepal time
NepaliDate     today = BsAdConverter.Today;          // BS date only

DateTime  ad  = BsAdConverter.BsToAd(new NepaliDate(2082, 1, 15));   // BS → AD DateTime
NepaliDate bs = BsAdConverter.AdToBs(DateTime.UtcNow);               // AD → BS

DateOnly adOnly = BsAdConverter.BsToAdDateOnly(new NepaliDate(2082, 1, 15));
NepaliDate bs2  = BsAdConverter.AdToBs(DateOnly.FromDateTime(DateTime.Today));

BsAdConverter.FormatAdEquivalent(new NepaliDate(2082, 1, 15)); // "28 April 2025"
BsAdConverter.FormatBsEquivalent(DateTime.Today);              // "15 Baisakh 2082"
```

---

## NepaliDateFormatter

Token-based formatting for both `NepaliDate` and `NepaliDateTime`.

```csharp
using NepaliUtility.Formatting;

var date = new NepaliDate(2082, 1, 15);
var dt   = BsAdConverter.Now; // NepaliDateTime

NepaliDateFormatter.Format(date, "EEEE, d MMMM yyyy")          // "Tuesday, 15 Baisakh 2082"
NepaliDateFormatter.Format(date, "d MMMM yyyy", nepali: true)  // "१५ बैशाख २०८२"
NepaliDateFormatter.Format(dt,   "yyyy.MM.dd GGG 'at' HH:mm:ss")
// "2082.01.15 Bikram Sambat at 14:30:00"

// Extension methods
date.Format("d MMMM yyyy")
dt.Format("h:mm aa", nepali: true)  // "२:३० बेलुकी"
```

### Date tokens

| Token | Output |
|---|---|
| `G` / `GG` / `GGG` | Era: `BS` / `B.S.` / `Bikram Sambat` |
| `y` / `yy` / `yyyy` | Year: `2082` / `82` / `2082` |
| `Q` / `QQ` / `QQQ` / `QQQQ` | Quarter: `1` / `01` / `Q1` / `1st quarter` |
| `M` / `MM` / `MMM` / `MMMM` / `MMMMM` | Month: `1` / `01` / `Bai` / `Baisakh` / `Vaishakha` |
| `d` / `dd` | Day: `5` / `05` |
| `E` / `EE` / `EEE` / `EEEE` | Weekday: `Sat` / `Sa` / `Sat` / `Saturday` |

### Time tokens

| Token | Output |
|---|---|
| `H` / `HH` | 24-hour: `14` / `14` |
| `h` / `hh` | 12-hour: `2` / `02` |
| `a` / `aa` | AM/PM: `pm` / `PM` (Nepali: `बिहान` / `बेलुकी`) |
| `m` / `mm` | Minute: `3` / `03` |
| `s` / `ss` | Second: `5` / `05` |
| `S` | Millisecond: `978` |

Wrap literal text in single quotes: `"'Miti:' d MMMM yyyy"` → `"Miti: 15 Baisakh 2082"`.
Use `''` for a literal apostrophe.

---

## NepaliDateFormat — predefined shortcuts

```csharp
using NepaliUtility.Formatting;

var dt = BsAdConverter.Now;

NepaliDateFormat.MEd().Format(dt)         // "Sat, 4/18"
NepaliDateFormat.MMMMEEEEd().Format(dt)   // "Saturday, Shrawan 18"
NepaliDateFormat.yMMMd().Format(dt)       // "Shr 18, 2082"
NepaliDateFormat.jms().Format(dt)         // "9:17:56 PM"
NepaliDateFormat.Hms().Format(dt)         // "21:17:56"

// Custom pattern
new NepaliDateFormat("EEE, MMM d, ''yy").Format(dt)  // "Sat, Bai 12, '82"
```

**All predefined formats:** `d` `E` `EEEE` `LLL` `LLLL` `M` `Md` `MEd` `MMM` `MMMd` `MMMEd` `MMMM` `MMMMd` `MMMMEEEEd` `QQQ` `QQQQ` `y` `yM` `yMd` `yMEd` `yMMM` `yMMMd` `yMMMEd` `yMMMMM` `yMMMMd` `yMMMMEEEEd` `yQQQ` `yQQQQ` `H` `Hm` `Hms` `j` `jm` `jms` `m` `ms` `s`

---

## NepaliMoment — relative time

```csharp
using NepaliUtility.Formatting;

NepaliMoment.Elapsed(date)                // "3 days ago"
NepaliMoment.Elapsed(date, nepali: true)  // "३ दिन पहिले"
NepaliMoment.Elapsed(DateTime.Today.AddDays(-8)) // "1 week ago"

date.Elapsed()
date.Elapsed(nepali: true)
```

| Range | English | Nepali |
|---|---|---|
| < 1 minute | `Just now` | `भर्खरै` |
| < 1 hour | `X minutes ago` | `X मिनेट पहिले` |
| < 1 day | `X hours ago` | `X घण्टा पहिले` |
| 1 day | `Yesterday` / `Tomorrow` | `हिजो` / `भोलि` |
| < 7 days | `X days ago` | `X दिन पहिले` |
| < 30 days | `X weeks ago` | `X हप्ता पहिले` |
| < 365 days | `X months ago` | `X महिना पहिले` |
| 365+ days | `X years ago` | `X वर्ष पहिले` |

---

## NepaliNumberFormat

Nepali comma grouping (`1,23,456`), currency symbol, decimal support, and in-words conversion.

```csharp
using NepaliUtility.Formatting;

// Comma-separated (Nepali grouping: last 3, then groups of 2)
new NepaliNumberFormat().Format(123456);                      // "1,23,456"
new NepaliNumberFormat().Format(123456789.6548);              // "12,34,56,789"

// With symbol
new NepaliNumberFormat(symbol: "Rs.").Format(123456);         // "Rs. 1,23,456"

// With decimals
new NepaliNumberFormat(decimalDigits: 2).Format(123456789.6548);
// "12,34,56,789.65"

// In words — English denominations
new NepaliNumberFormat(inWords: true).Format(123456);
// "1 lakh 23 thousand 4 hundred 56"

// In words — Nepali (Devanagari digits + Nepali labels)
new NepaliNumberFormat(inWords: true, language: Language.Nepali).Format(123456);
// "१ लाख २३ हजार ४ सय ५६"

// Monetary in Nepali
new NepaliNumberFormat(
    inWords: true, language: Language.Nepali,
    isMonetary: true, decimalDigits: 2).Format(123456789.65);
// "१२ करोड ३४ लाख ५६ हजार ७ सय ८९ रुपैया ६५ पैसा"
```

**Denomination scale:** हजार (1,000) → लाख (1,00,000) → करोड (1,00,00,000) → अर्ब (1,00,00,00,000)

---

## NepaliNumberConverter — Devanagari numerals

```csharp
using NepaliUtility.Formatting;

NepaliNumberConverter.ToDevanagari(2082)          // "२०८२"
NepaliNumberConverter.ToDevanagari("2082/01/15")  // "२०८२/०१/१५"
NepaliNumberConverter.ToEnglish("२०८२")           // "2082"
```

---

## NepaliFiscalYear

Nepal's fiscal year runs from **1 Shrawan (month 4)** to the **last day of Ashad (month 3)** of the following year.

```csharp
using NepaliUtility.Services;

var date = new NepaliDate(2082, 5, 10);

NepaliFiscalYear.GetFiscalYear(date)           // 2082
NepaliFiscalYear.GetFiscalYearString(date)     // "2082/83"
NepaliFiscalYear.GetFiscalYearStartDate(2082)  // NepaliDate(2082, 4, 1)
NepaliFiscalYear.GetFiscalYearEndDate(2082)    // NepaliDate(2083, 3, 32)
NepaliFiscalYear.IsSameFiscalYear(
    new NepaliDate(2082, 4, 1),
    new NepaliDate(2083, 3, 1))                // true
```

---

## BsCalendarData

```csharp
using NepaliUtility.Data;

BsCalendarData.MinYear              // 1970
BsCalendarData.MaxYear              // 2100
BsCalendarData.GetDaysInMonth(2082, 1)   // 31
BsCalendarData.GetDaysInYear(2082)       // 365
BsCalendarData.GetMonthDays(2082)        // int[12]
BsCalendarData.IsYearSupported(2082)     // true
BsCalendarData.ClampDay(2082, 1, 99)     // 31
```

---

## License

Released under the [MIT License](https://opensource.org/licenses/MIT). Free to use, modify, and distribute.

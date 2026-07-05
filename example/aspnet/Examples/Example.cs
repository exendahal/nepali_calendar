using NepaliUtility.Formatting;
using NepaliUtility.Models;
using NepaliUtility.Services;

namespace NepaliUtilityDemo.AspNet.Examples;

/// <summary>
/// Exercises every public API in NepaliUtility.Core and returns the results
/// grouped by type, so they can be rendered as a live reference page.
/// </summary>
public static class Example
{
    public static IReadOnlyList<ExampleSection> Run()
    {
        var today = BsAdConverter.Today;
        var sample = new NepaliDate(2081, 4, 18);
        NepaliDate.TryParse("2081-04-18", out var parsedSample);

        return new[]
        {
            new ExampleSection(
                "NepaliUtility.Services",
                "BsAdConverter",
                "Converts between the Bikram Sambat (BS) and Gregorian (AD) calendars.",
                new ExampleItem[]
                {
                    new("Today", "BsAdConverter.Today", today.ToString()),
                    new("Now", "BsAdConverter.Now", BsAdConverter.Now.ToString()),
                    new("AdToBs(DateTime)", "BsAdConverter.AdToBs(new DateTime(2024, 8, 2))",
                        BsAdConverter.AdToBs(new DateTime(2024, 8, 2)).ToString()),
                    new("AdToBs(DateOnly)", "BsAdConverter.AdToBs(new DateOnly(2024, 8, 2))",
                        BsAdConverter.AdToBs(new DateOnly(2024, 8, 2)).ToString()),
                    new("BsToAd", "BsAdConverter.BsToAd(new NepaliDate(2081, 4, 18))",
                        BsAdConverter.BsToAd(sample).ToString("yyyy-MM-dd")),
                    new("BsToAdDateOnly", "BsAdConverter.BsToAdDateOnly(new NepaliDate(2081, 4, 18))",
                        BsAdConverter.BsToAdDateOnly(sample).ToString()),
                    new("FormatAdEquivalent", "BsAdConverter.FormatAdEquivalent(new NepaliDate(2081, 4, 18))",
                        BsAdConverter.FormatAdEquivalent(sample)),
                    new("FormatBsEquivalent", "BsAdConverter.FormatBsEquivalent(new DateTime(2024, 8, 2))",
                        BsAdConverter.FormatBsEquivalent(new DateTime(2024, 8, 2))),
                }),

            new ExampleSection(
                "NepaliUtility.Models",
                "NepaliDate",
                "Immutable BS date value with parsing, arithmetic, and display helpers.",
                new ExampleItem[]
                {
                    new("ToString", "sample.ToString()", sample.ToString()),
                    new("ToDisplayString", "sample.ToDisplayString()", sample.ToDisplayString()),
                    new("MonthName", "sample.MonthName", sample.MonthName),
                    new("MonthNameNepali", "sample.MonthNameNepali", sample.MonthNameNepali),
                    new("IsValid", "sample.IsValid()", sample.IsValid().ToString()),
                    new("AddMonths", "sample.AddMonths(2)", sample.AddMonths(2).ToString()),
                    new("AddYears", "sample.AddYears(1)", sample.AddYears(1).ToString()),
                    new("Parse", "NepaliDate.Parse(\"2081/04/18\")", NepaliDate.Parse("2081/04/18").ToString()),
                    new("TryParse", "NepaliDate.TryParse(\"2081-04-18\", out var d)", parsedSample?.ToString() ?? "—"),
                    new("operator <", "sample < sample.AddDays(1)", (sample < sample.AddDays(1)).ToString()),
                }),

            new ExampleSection(
                "NepaliUtility.Models",
                "NepaliDateTime",
                "A BS date combined with a time-of-day component.",
                new ExampleItem[]
                {
                    new("ToString", "new NepaliDateTime(sample, new TimeOnly(21, 4, 5))",
                        new NepaliDateTime(sample, new TimeOnly(21, 4, 5)).ToString()),
                    new("Parse", "NepaliDateTime.Parse(\"2081/04/18 21:04:05\")",
                        NepaliDateTime.Parse("2081/04/18 21:04:05").ToString()),
                    new("TryParse", "NepaliDateTime.TryParse(\"2081/04/18 09:30\", out var dt)",
                        NepaliDateTime.TryParse("2081/04/18 09:30", out var dt) ? dt!.ToString() : "—"),
                }),

            new ExampleSection(
                "NepaliUtility.Formatting",
                "NepaliDateExtensions",
                "Fluent extension methods on NepaliDate / NepaliDateTime (using NepaliUtility.Formatting;).",
                new ExampleItem[]
                {
                    new("Format", "sample.Format(\"EEEE, d MMMM yyyy\")", sample.Format("EEEE, d MMMM yyyy")),
                    new("Format (nepali)", "sample.Format(\"EEEE, d MMMM yyyy\", nepali: true)",
                        sample.Format("EEEE, d MMMM yyyy", nepali: true)),
                    new("Elapsed", "sample.Elapsed()", sample.Elapsed()),
                    new("AddDays", "sample.AddDays(10)", sample.AddDays(10).ToString()),
                    new("GetDayOfWeek", "sample.GetDayOfWeek()", sample.GetDayOfWeek().ToString()),
                    new("DaysTo", "sample.DaysTo(sample.AddDays(30))", sample.DaysTo(sample.AddDays(30)).ToString()),
                }),

            new ExampleSection(
                "NepaliUtility.Formatting",
                "NepaliDateFormatter",
                "Token-based pattern formatter used internally by Format() and NepaliDateFormat.",
                new ExampleItem[]
                {
                    new("d MMMM yyyy", "NepaliDateFormatter.Format(sample, \"d MMMM yyyy\")",
                        NepaliDateFormatter.Format(sample, "d MMMM yyyy")),
                    new("dd/MM/yyyy", "NepaliDateFormatter.Format(sample, \"dd/MM/yyyy\")",
                        NepaliDateFormatter.Format(sample, "dd/MM/yyyy")),
                    new("EEEE, d MMMM yyyy", "NepaliDateFormatter.Format(sample, \"EEEE, d MMMM yyyy\")",
                        NepaliDateFormatter.Format(sample, "EEEE, d MMMM yyyy")),
                    new("QQQQ yyyy", "NepaliDateFormatter.Format(sample, \"QQQQ yyyy\")",
                        NepaliDateFormatter.Format(sample, "QQQQ yyyy")),
                    new("with era (GGG)", "NepaliDateFormatter.Format(sample, \"d MMMM yyyy GGG\")",
                        NepaliDateFormatter.Format(sample, "d MMMM yyyy GGG")),
                    new("nepali digits", "NepaliDateFormatter.Format(sample, \"yyyy/MM/dd\", nepali: true)",
                        NepaliDateFormatter.Format(sample, "yyyy/MM/dd", nepali: true)),
                }),

            new ExampleSection(
                "NepaliUtility.Formatting",
                "NepaliDateFormat",
                "Predefined format shortcuts, modeled after ICU/Moment-style skeletons.",
                new ExampleItem[]
                {
                    new("yMMMMEEEEd", "NepaliDateFormat.yMMMMEEEEd().Format(sample)",
                        NepaliDateFormat.yMMMMEEEEd().Format(sample)),
                    new("MMMEd", "NepaliDateFormat.MMMEd().Format(sample)", NepaliDateFormat.MMMEd().Format(sample)),
                    new("yMMMd", "NepaliDateFormat.yMMMd().Format(sample)", NepaliDateFormat.yMMMd().Format(sample)),
                    new("yQQQ", "NepaliDateFormat.yQQQ().Format(sample)", NepaliDateFormat.yQQQ().Format(sample)),
                    new("jm (time)", "NepaliDateFormat.jm().Format(dateTime)",
                        NepaliDateFormat.jm().Format(new NepaliDateTime(sample, new TimeOnly(21, 4)))),
                    new("Hms (time)", "NepaliDateFormat.Hms().Format(dateTime)",
                        NepaliDateFormat.Hms().Format(new NepaliDateTime(sample, new TimeOnly(21, 4, 5)))),
                    new("custom pattern", "new NepaliDateFormat(\"yyyy.MM.dd GGG 'at' HH:mm:ss\").Format(dateTime)",
                        new NepaliDateFormat("yyyy.MM.dd GGG 'at' HH:mm:ss")
                            .Format(new NepaliDateTime(sample, new TimeOnly(11, 56, 25)))),
                }),

            new ExampleSection(
                "NepaliUtility.Formatting",
                "NepaliMoment",
                "Human-readable relative-time strings, e.g. \"3 days ago\".",
                new ExampleItem[]
                {
                    new("Elapsed (past BS date)", "NepaliMoment.Elapsed(sample)", NepaliMoment.Elapsed(sample)),
                    new("Elapsed (past, nepali)", "NepaliMoment.Elapsed(sample, nepali: true)",
                        NepaliMoment.Elapsed(sample, nepali: true)),
                    new("Elapsed (future BS date)", "NepaliMoment.Elapsed(sample.AddDays(3), today)",
                        NepaliMoment.Elapsed(sample.AddDays(3), today)),
                    new("Elapsed (AD DateTime)", "NepaliMoment.Elapsed(DateTime.Today.AddHours(-5))",
                        NepaliMoment.Elapsed(DateTime.Today.AddHours(-5))),
                }),

            new ExampleSection(
                "NepaliUtility.Formatting",
                "NepaliNumberConverter",
                "Converts digits between ASCII and Devanagari numerals.",
                new ExampleItem[]
                {
                    new("ToDevanagari(int)", "NepaliNumberConverter.ToDevanagari(123456)",
                        NepaliNumberConverter.ToDevanagari(123456)),
                    new("ToDevanagari(string)", "NepaliNumberConverter.ToDevanagari(\"2081-04-18\")",
                        NepaliNumberConverter.ToDevanagari("2081-04-18")),
                    new("ToEnglish", "NepaliNumberConverter.ToEnglish(\"२०८१-०४-१८\")",
                        NepaliNumberConverter.ToEnglish("२०८१-०४-१८")),
                }),

            new ExampleSection(
                "NepaliUtility.Formatting",
                "NepaliNumberFormat",
                "Nepali comma grouping, currency symbols, and number-to-words conversion.",
                new ExampleItem[]
                {
                    new("comma grouping", "new NepaliNumberFormat().Format(123456)",
                        new NepaliNumberFormat().Format(123456)),
                    new("comma grouping (nepali)",
                        "new NepaliNumberFormat(language: Language.Nepali).Format(123456)",
                        new NepaliNumberFormat(language: Language.Nepali).Format(123456)),
                    new("currency symbol", "new NepaliNumberFormat(symbol: \"Rs.\").Format(123456)",
                        new NepaliNumberFormat(symbol: "Rs.").Format(123456)),
                    new("currency symbol (nepali)",
                        "new NepaliNumberFormat(symbol: \"रु.\", language: Language.Nepali).Format(123456)",
                        new NepaliNumberFormat(symbol: "रु.", language: Language.Nepali).Format(123456)),
                    new("decimals", "new NepaliNumberFormat(decimalDigits: 2).Format(123456.75)",
                        new NepaliNumberFormat(decimalDigits: 2).Format(123456.75)),
                    new("decimals (nepali)",
                        "new NepaliNumberFormat(decimalDigits: 2, language: Language.Nepali).Format(123456.75)",
                        new NepaliNumberFormat(decimalDigits: 2, language: Language.Nepali).Format(123456.75)),
                    new("in words (english)", "new NepaliNumberFormat(inWords: true).Format(123456)",
                        new NepaliNumberFormat(inWords: true).Format(123456)),
                    new("in words (nepali)",
                        "new NepaliNumberFormat(inWords: true, language: Language.Nepali).Format(123456)",
                        new NepaliNumberFormat(inWords: true, language: Language.Nepali).Format(123456)),
                    new("in words, monetary (nepali)",
                        "new NepaliNumberFormat(inWords: true, language: Language.Nepali, isMonetary: true, decimalDigits: 2).Format(123456789.65)",
                        new NepaliNumberFormat(inWords: true, language: Language.Nepali, isMonetary: true, decimalDigits: 2)
                            .Format(123456789.65)),
                }),

            new ExampleSection(
                "NepaliUtility.Services",
                "NepaliFiscalYear",
                "Nepal's government fiscal year: 1 Shrawan to the last day of Ashad.",
                new ExampleItem[]
                {
                    new("GetFiscalYear", "NepaliFiscalYear.GetFiscalYear(sample)",
                        NepaliFiscalYear.GetFiscalYear(sample).ToString()),
                    new("GetFiscalYearString", "NepaliFiscalYear.GetFiscalYearString(sample)",
                        NepaliFiscalYear.GetFiscalYearString(sample)),
                    new("GetFiscalYearStartDate", "NepaliFiscalYear.GetFiscalYearStartDate(2081)",
                        NepaliFiscalYear.GetFiscalYearStartDate(2081).ToString()),
                    new("GetFiscalYearEndDate", "NepaliFiscalYear.GetFiscalYearEndDate(2081)",
                        NepaliFiscalYear.GetFiscalYearEndDate(2081).ToString()),
                    new("IsSameFiscalYear", "NepaliFiscalYear.IsSameFiscalYear(sample, sample.AddMonths(2))",
                        NepaliFiscalYear.IsSameFiscalYear(sample, sample.AddMonths(2)).ToString()),
                }),

            new ExampleSection(
                "NepaliUtility.Services",
                "NepaliTimeZone",
                "Nepal Standard Time (UTC+5:45) helpers.",
                new ExampleItem[]
                {
                    new("Offset", "NepaliTimeZone.Offset", NepaliTimeZone.Offset.ToString()),
                    new("Now", "NepaliTimeZone.Now", NepaliTimeZone.Now.ToString("yyyy-MM-dd HH:mm:ss zzz")),
                    new("ToNepalTime(UTC)", "NepaliTimeZone.ToNepalTime(DateTime.UtcNow)",
                        NepaliTimeZone.ToNepalTime(DateTime.UtcNow).ToString("yyyy-MM-dd HH:mm:ss zzz")),
                }),
        };
    }
}

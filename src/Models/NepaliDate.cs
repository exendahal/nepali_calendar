namespace NepaliDatePicker.Models;

/// <summary>
/// Represents a date in the Bikram Sambat (BS) calendar system.
/// </summary>
public record NepaliDate(int Year, int Month, int Day)
{
    public static readonly string[] MonthNames =
    [
        "Baisakh", "Jestha", "Ashadh", "Shrawan",
        "Bhadra", "Ashwin", "Kartik", "Mangsir",
        "Poush",   "Magh",   "Falgun", "Chaitra"
    ];

    public static readonly string[] MonthNamesNepali =
    [
        "बैशाख", "जेठ", "असार", "श्रावण",
        "भाद्र", "आश्विन", "कार्तिक", "मंसिर",
        "पुष",   "माघ",   "फाल्गुन", "चैत्र"
    ];

    public string MonthName => MonthNames[Month - 1];
    public string MonthNameNepali => MonthNamesNepali[Month - 1];

    public override string ToString() => $"{Year}/{Month:D2}/{Day:D2}";

    public string ToDisplayString() => $"{Day} {MonthName} {Year}";

    public bool IsValid()
    {
        if (Month < 1 || Month > 12) return false;
        if (Day < 1) return false;
        try
        {
            int maxDay = Data.BsCalendarData.GetDaysInMonth(Year, Month);
            return Day <= maxDay;
        }
        catch
        {
            return false;
        }
    }
}

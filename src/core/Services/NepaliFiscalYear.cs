using NepaliUtility.Data;
using NepaliUtility.Models;

namespace NepaliUtility.Services;

/// <summary>
/// Helpers for Nepal's government fiscal year, which runs from
/// 1 Shrawan (month 4) of year N to the last day of Ashad (month 3) of year N+1.
/// Example: FY 2081/82 = 1 Shrawan 2081 → last day of Ashad 2082.
/// </summary>
public static class NepaliFiscalYear
{
    private const int StartMonth = 4; // Shrawan

    /// <summary>
    /// Returns the fiscal year start year for <paramref name="date"/>.
    /// Dates in Shrawan–Chaitra belong to the fiscal year that started that year;
    /// dates in Baisakh–Ashad belong to the fiscal year that started the previous year.
    /// </summary>
    public static int GetFiscalYear(NepaliDate date)
        => date.Month >= StartMonth ? date.Year : date.Year - 1;

    /// <summary>
    /// Returns the fiscal year as a display string, e.g. <c>"2081/82"</c>.
    /// </summary>
    public static string GetFiscalYearString(NepaliDate date)
    {
        int fy = GetFiscalYear(date);
        return $"{fy}/{(fy + 1) % 100:D2}";
    }

    /// <summary>Returns the first day of the fiscal year (1 Shrawan of <paramref name="fiscalYear"/>).</summary>
    public static NepaliDate GetFiscalYearStartDate(int fiscalYear)
        => new NepaliDate(fiscalYear, StartMonth, 1);

    /// <summary>Returns the last day of the fiscal year (last day of Ashad of <paramref name="fiscalYear"/> + 1).</summary>
    public static NepaliDate GetFiscalYearEndDate(int fiscalYear)
    {
        int endYear  = fiscalYear + 1;
        int endMonth = StartMonth - 1; // Ashad = month 3
        int lastDay  = BsCalendarData.GetDaysInMonth(endYear, endMonth);
        return new NepaliDate(endYear, endMonth, lastDay);
    }

    /// <summary>Returns <c>true</c> if both dates fall in the same fiscal year.</summary>
    public static bool IsSameFiscalYear(NepaliDate a, NepaliDate b)
        => GetFiscalYear(a) == GetFiscalYear(b);
}

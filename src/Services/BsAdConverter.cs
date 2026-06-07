using NepaliDatePicker.Data;
using NepaliDatePicker.Models;

namespace NepaliDatePicker.Services;

/// <summary>
/// Converts dates between BS (Bikram Sambat) and AD (Anno Domini / Gregorian) calendars.
/// Reference epoch: BS 2000/01/01 = AD 1943/04/14.
/// </summary>
public static class BsAdConverter
{
    // Epoch: 1 Baisakh 2000 BS = 4 April 1943 AD
    // Validated: BS 2083/02/23 (Jestha 23) = AD 2026/06/06 ✓
    private static readonly DateTime AdEpoch = new(1943, 4, 4);
    private const int BsEpochYear = 2000;
    private const int BsEpochMonth = 1;
    private const int BsEpochDay = 1;

    /// <summary>Converts a BS date to its equivalent AD date.</summary>
    public static DateTime BsToAd(NepaliDate bs)
    {
        int totalDays = DaysFromBsEpoch(bs.Year, bs.Month, bs.Day);
        return AdEpoch.AddDays(totalDays);
    }

    /// <summary>Converts an AD date to its equivalent BS date.</summary>
    public static NepaliDate AdToBs(DateTime ad)
    {
        int totalDays = (ad.Date - AdEpoch).Days;
        return DaysToBS(totalDays);
    }

    // Counts days from BS epoch (2000/01/01) to the given BS date (can be negative).
    private static int DaysFromBsEpoch(int year, int month, int day)
    {
        int days = 0;

        if (year >= BsEpochYear)
        {
            // Count forward from epoch year
            for (int y = BsEpochYear; y < year; y++)
                days += BsCalendarData.GetDaysInYear(y);

            for (int m = BsEpochMonth; m < month; m++)
                days += BsCalendarData.GetDaysInMonth(year, m);

            days += day - BsEpochDay;
        }
        else
        {
            // Count backward from epoch year
            for (int y = BsEpochYear - 1; y >= year; y--)
                days -= BsCalendarData.GetDaysInYear(y);

            // Now add back partial year
            int daysInYear = 0;
            for (int m = 1; m < month; m++)
                daysInYear += BsCalendarData.GetDaysInMonth(year, m);
            daysInYear += day - 1;

            days += daysInYear;
        }

        return days;
    }

    private static NepaliDate DaysToBS(int totalDays)
    {
        int year = BsEpochYear;
        int month = BsEpochMonth;
        int day = BsEpochDay;

        if (totalDays >= 0)
        {
            // Walk forward
            while (totalDays > 0)
            {
                int daysInMonth = BsCalendarData.GetDaysInMonth(year, month);
                if (totalDays < daysInMonth - (day - 1))
                {
                    day += totalDays;
                    totalDays = 0;
                }
                else
                {
                    totalDays -= daysInMonth - (day - 1);
                    day = 1;
                    month++;
                    if (month > 12)
                    {
                        month = 1;
                        year++;
                    }
                }
            }
        }
        else
        {
            // Walk backward
            totalDays = -totalDays;
            while (totalDays > 0)
            {
                if (totalDays < day)
                {
                    day -= totalDays;
                    totalDays = 0;
                }
                else
                {
                    totalDays -= day;
                    month--;
                    if (month < 1)
                    {
                        month = 12;
                        year--;
                    }
                    day = BsCalendarData.GetDaysInMonth(year, month);
                }
            }
        }

        return new NepaliDate(year, month, day);
    }

    /// <summary>Returns the AD date string for display alongside the BS picker.</summary>
    public static string FormatAdEquivalent(NepaliDate bs)
    {
        try
        {
            DateTime ad = BsToAd(bs);
            return ad.ToString("dd MMMM yyyy");
        }
        catch
        {
            return "—";
        }
    }

    /// <summary>Returns the BS date string for display alongside the AD picker.</summary>
    public static string FormatBsEquivalent(DateTime ad)
    {
        try
        {
            NepaliDate bs = AdToBs(ad);
            return bs.ToDisplayString();
        }
        catch
        {
            return "—";
        }
    }
}

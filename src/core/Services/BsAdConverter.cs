using NepaliUtility.Data;
using NepaliUtility.Models;

namespace NepaliUtility.Services;

/// <summary>
/// Converts dates between BS (Bikram Sambat) and AD (Anno Domini / Gregorian) calendars.
/// Reference epoch: BS 2000/01/01 = AD 1943/04/14.
/// </summary>
public static class BsAdConverter
{
    private static readonly DateTime _AdEpoch = new(1943, 4, 14);
    private const int _BsEpochYear = 2000;
    private const int _BsEpochMonth = 1;
    private const int _BsEpochDay = 1;

    /// <summary>Converts a BS date to its equivalent AD date.</summary>
    public static DateTime BsToAd(NepaliDate bs)
    {
        int totalDays = DaysFromBsEpoch(bs.Year, bs.Month, bs.Day);
        return _AdEpoch.AddDays(totalDays);
    }

    /// <summary>Converts an AD date to its equivalent BS date.</summary>
    public static NepaliDate AdToBs(DateTime ad)
    {
        int totalDays = (ad.Date - _AdEpoch).Days;
        return DaysToBS(totalDays);
    }

    // Counts days from BS epoch (2000/01/01) to the given BS date (can be negative).
    private static int DaysFromBsEpoch(int year, int month, int day)
    {
        int days = 0;

        if (year >= _BsEpochYear)
        {
            // Count forward from epoch year
            for (int y = _BsEpochYear; y < year; y++)
                days += BsCalendarData.GetDaysInYear(y);

            for (int m = _BsEpochMonth; m < month; m++)
                days += BsCalendarData.GetDaysInMonth(year, m);

            days += day - _BsEpochDay;
        }
        else
        {
            // Count backward from epoch year
            for (int y = _BsEpochYear - 1; y >= year; y--)
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
        int year = _BsEpochYear;
        int month = _BsEpochMonth;
        int day = _BsEpochDay;

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

    /// <summary>Returns today's date in Bikram Sambat.</summary>
    public static NepaliDate Today => AdToBs(DateTime.Today);

    /// <summary>Converts a <see cref="DateOnly"/> AD date to its equivalent BS date.</summary>
    public static NepaliDate AdToBs(DateOnly date) => AdToBs(date.ToDateTime(TimeOnly.MinValue));

    /// <summary>Converts a BS date to its equivalent <see cref="DateOnly"/> AD date.</summary>
    public static DateOnly BsToAdDateOnly(NepaliDate bs) => DateOnly.FromDateTime(BsToAd(bs));

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

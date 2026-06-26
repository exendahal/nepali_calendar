namespace NepaliUtility.Formatting;

/// <summary>
/// Converts between ASCII digits and Devanagari (Nepali) numerals.
/// </summary>
public static class NepaliNumberConverter
{
    /// <summary>Converts an integer to a Devanagari numeral string.</summary>
    public static string ToDevanagari(int number) => ToDevanagari(number.ToString());

    /// <summary>
    /// Converts ASCII digits in <paramref name="s"/> to Devanagari numerals (०–९).
    /// Non-digit characters (e.g. separators) are passed through unchanged.
    /// </summary>
    public static string ToDevanagari(string s)
    {
        var sb = new System.Text.StringBuilder(s.Length);
        foreach (char c in s)
            sb.Append(c is >= '0' and <= '9' ? (char)('०' + (c - '0')) : c);
        return sb.ToString();
    }

    /// <summary>
    /// Converts Devanagari numerals (०–९) in <paramref name="s"/> back to ASCII digits.
    /// Non-Devanagari characters are passed through unchanged.
    /// </summary>
    public static string ToEnglish(string s)
    {
        var sb = new System.Text.StringBuilder(s.Length);
        foreach (char c in s)
            sb.Append(c >= '०' && c <= '९' ? (char)('0' + (c - '०')) : c);
        return sb.ToString();
    }
}

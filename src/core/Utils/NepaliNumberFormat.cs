namespace NepaliUtility.Formatting;

/// <summary>Language used for number-in-words output.</summary>
public enum Language { English, Nepali }

/// <summary>
/// Formats numbers with Nepali comma grouping, optional currency symbol,
/// and in-words conversion using the Nepali denomination system
/// (hajar → lakh → crore → arba).
/// </summary>
/// <example>
/// <code>
/// // Comma-separated (Nepali style: 1,23,456)
/// new NepaliNumberFormat().Format(123456);                     // "1,23,456"
///
/// // With symbol
/// new NepaliNumberFormat(symbol: "Rs.").Format(123456);        // "Rs. 1,23,456"
///
/// // With decimals
/// new NepaliNumberFormat(decimalDigits: 2).Format(123456.75);  // "1,23,456.75"
///
/// // In words — English denominations
/// new NepaliNumberFormat(inWords: true).Format(123456);
/// // "1 lakh 23 thousand 4 hundred 56"
///
/// // In words — Nepali (Devanagari digits + Nepali denomination labels)
/// new NepaliNumberFormat(inWords: true, language: Language.Nepali).Format(123456);
/// // "१ लाख २३ हजार ४ सय ५६"
///
/// // Monetary in Nepali
/// new NepaliNumberFormat(inWords: true, language: Language.Nepali,
///     isMonetary: true, decimalDigits: 2).Format(123456789.65);
/// // "१२ करोड ३४ लाख ५६ हजार ७ सय ८९ रुपैया ६५ पैसा"
/// </code>
/// </example>
public sealed class NepaliNumberFormat
{
    private readonly string?  _symbol;
    private readonly int      _decimalDigits;
    private readonly bool     _inWords;
    private readonly Language _language;
    private readonly bool     _isMonetary;

    public NepaliNumberFormat(
        string?  symbol        = null,
        int      decimalDigits = 0,
        bool     inWords       = false,
        Language language      = Language.English,
        bool     isMonetary    = false)
    {
        _symbol        = symbol;
        _decimalDigits = Math.Max(0, decimalDigits);
        _inWords       = inWords;
        _language      = language;
        _isMonetary    = isMonetary;
    }

    public string Format(double number)  => _inWords ? FormatInWords(number) : FormatNumeric(number);
    public string Format(decimal number) => Format((double)number);
    public string Format(long number)    => Format((double)number);
    public string Format(int number)     => Format((double)number);

    // ── Numeric (comma-separated) ─────────────────────────────────────────────

    private string FormatNumeric(double number)
    {
        bool negative = number < 0;
        double abs = Math.Abs(number);
        long intPart = (long)abs;

        string intStr = NepalCommas(intPart);
        string result = intStr;

        if (_decimalDigits > 0)
        {
            string rounded = abs.ToString("F" + _decimalDigits);
            int dot = rounded.IndexOf('.');
            string frac = dot >= 0 ? rounded[(dot + 1)..] : new string('0', _decimalDigits);
            result = $"{intStr}.{frac}";
        }

        if (_language == Language.Nepali)
            result = NepaliNumberConverter.ToDevanagari(result);

        if (_symbol is not null)
            result = $"{_symbol} {result}";

        return negative ? $"-{result}" : result;
    }

    // Nepali comma grouping: last 3 digits, then groups of 2 from the right.
    private static string NepalCommas(long n)
    {
        if (n < 1000) return n.ToString();
        string s = n.ToString();
        var groups = new System.Collections.Generic.List<string>();
        groups.Add(s[^3..]);
        s = s[..^3];
        while (s.Length > 0)
        {
            int take = Math.Min(2, s.Length);
            groups.Add(s[^take..]);
            s = s[..^take];
        }
        groups.Reverse();
        return string.Join(",", groups);
    }

    // ── In-words ──────────────────────────────────────────────────────────────

    private string FormatInWords(double number)
    {
        bool negative = number < 0;
        double abs = Math.Abs(number);
        long intPart = (long)abs;

        string words = _language == Language.Nepali
            ? ToWordsNepali(intPart)
            : ToWordsEnglish(intPart);

        if (_isMonetary)
        {
            string currencyLabel = _language == Language.Nepali ? "रुपैया" : "rupees";
            words = $"{words} {currencyLabel}";

            if (_decimalDigits > 0)
            {
                double frac = abs - intPart;
                long subunit = (long)Math.Round(frac * Math.Pow(10, _decimalDigits));
                if (subunit > 0)
                {
                    string paisaLabel = _language == Language.Nepali ? "पैसा" : "paisa";
                    string subStr = _language == Language.Nepali
                        ? NepaliNumberConverter.ToDevanagari(subunit.ToString())
                        : subunit.ToString();
                    words = $"{words} {subStr} {paisaLabel}";
                }
            }
        }

        return negative ? $"-{words}" : words;
    }

    private static string ToWordsEnglish(long n)
    {
        if (n == 0) return "0";
        var parts = new System.Collections.Generic.List<string>();
        Chunk(ref n, 1_00_00_00_000L, "arba",    parts);
        Chunk(ref n, 1_00_00_000L,    "crore",    parts);
        Chunk(ref n, 1_00_000L,       "lakh",     parts);
        Chunk(ref n, 1_000L,          "thousand", parts);
        Chunk(ref n, 100L,            "hundred",  parts);
        if (n > 0) parts.Add(n.ToString());
        return string.Join(" ", parts);
    }

    private static string ToWordsNepali(long n)
    {
        if (n == 0) return NepaliNumberConverter.ToDevanagari("0");
        var parts = new System.Collections.Generic.List<string>();
        ChunkNp(ref n, 1_00_00_00_000L, "अर्ब",  parts);
        ChunkNp(ref n, 1_00_00_000L,    "करोड",  parts);
        ChunkNp(ref n, 1_00_000L,       "लाख",   parts);
        ChunkNp(ref n, 1_000L,          "हजार",  parts);
        ChunkNp(ref n, 100L,            "सय",    parts);
        if (n > 0) parts.Add(NepaliNumberConverter.ToDevanagari(n.ToString()));
        return string.Join(" ", parts);
    }

    private static void Chunk(ref long n, long unit, string label,
        System.Collections.Generic.List<string> parts)
    {
        if (n < unit) return;
        parts.Add($"{n / unit} {label}");
        n %= unit;
    }

    private static void ChunkNp(ref long n, long unit, string label,
        System.Collections.Generic.List<string> parts)
    {
        if (n < unit) return;
        parts.Add($"{NepaliNumberConverter.ToDevanagari((n / unit).ToString())} {label}");
        n %= unit;
    }
}

using System.Globalization;

namespace CopyDate;

internal enum CalendarSystem
{
    Gregorian,
    ChineseLunar,
    ThaiBuddhist,
    Japanese,
    Taiwan
}

internal enum CalendarLayout
{
    Gregorian,
    LunarMonthDay,
    LunarStemMonthDay,
    LunarFull,
    ThaiIso,
    ThaiSlash,
    ThaiEraYear,
    JapaneseDate,
    JapaneseDateTime,
    JapaneseShort,
    TaiwanNumeric,
    TaiwanFormal,
    TaiwanFull
}

internal sealed record DateFormatDefinition(
    string Id,
    string Pattern,
    bool IsBuiltIn,
    string? Name = null,
    CalendarSystem Calendar = CalendarSystem.Gregorian,
    CalendarLayout Layout = CalendarLayout.Gregorian);

internal static class DateFormatCatalog
{
    public const string IsoDate = "iso-date";
    public const string SlashDate = "slash-date";
    public const string ChineseDate = "cn-date";
    public const string CompactDate = "compact-date";
    public const string IsoDateTimeMinutes = "iso-datetime-min";
    public const string IsoDateTimeSeconds = "iso-datetime-sec";
    public const string ChineseDateTimeSeconds = "cn-datetime-sec";
    public const string LunarDate = "lunar-date";
    public const string LunarStem = "lunar-stem";
    public const string LunarFull = "lunar-full";
    public const string BuddhistDate = "be-date";
    public const string BuddhistSlash = "be-slash";
    public const string BuddhistEra = "be-era";
    public const string JapaneseDate = "jp-date";
    public const string JapaneseDateTime = "jp-datetime";
    public const string JapaneseShort = "jp-short";
    public const string TaiwanDate = "roc-date";
    public const string TaiwanFormal = "roc-formal";
    public const string TaiwanFull = "roc-full";

    public static IReadOnlyList<string> DefaultEnabledIds { get; } =
    [
        IsoDate,
        SlashDate,
        ChineseDate,
        CompactDate,
        IsoDateTimeMinutes,
        IsoDateTimeSeconds,
        ChineseDateTimeSeconds
    ];

    public static IReadOnlyList<DateFormatDefinition> BuiltIn { get; } =
    [
        Gregorian(IsoDate, "yyyy-MM-dd"),
        Gregorian(SlashDate, "yyyy/MM/dd"),
        Gregorian(ChineseDate, "yyyy年M月d日"),
        Gregorian(CompactDate, "yyyyMMdd"),
        Gregorian(IsoDateTimeMinutes, "yyyy-MM-dd HH:mm"),
        Gregorian(IsoDateTimeSeconds, "yyyy-MM-dd HH:mm:ss"),
        Gregorian(ChineseDateTimeSeconds, "yyyy年M月d日 HH:mm:ss"),
        Calendar(LunarDate, CalendarSystem.ChineseLunar, CalendarLayout.LunarMonthDay),
        Calendar(LunarStem, CalendarSystem.ChineseLunar, CalendarLayout.LunarStemMonthDay),
        Calendar(LunarFull, CalendarSystem.ChineseLunar, CalendarLayout.LunarFull),
        Calendar(BuddhistDate, CalendarSystem.ThaiBuddhist, CalendarLayout.ThaiIso),
        Calendar(BuddhistSlash, CalendarSystem.ThaiBuddhist, CalendarLayout.ThaiSlash),
        Calendar(BuddhistEra, CalendarSystem.ThaiBuddhist, CalendarLayout.ThaiEraYear),
        Calendar(JapaneseDate, CalendarSystem.Japanese, CalendarLayout.JapaneseDate),
        Calendar(JapaneseDateTime, CalendarSystem.Japanese, CalendarLayout.JapaneseDateTime),
        Calendar(JapaneseShort, CalendarSystem.Japanese, CalendarLayout.JapaneseShort),
        Calendar(TaiwanDate, CalendarSystem.Taiwan, CalendarLayout.TaiwanNumeric),
        Calendar(TaiwanFormal, CalendarSystem.Taiwan, CalendarLayout.TaiwanFormal),
        Calendar(TaiwanFull, CalendarSystem.Taiwan, CalendarLayout.TaiwanFull)
    ];

    public static string Format(string pattern, DateTime moment)
    {
        try
        {
            return moment.ToString(pattern, CultureInfo.CurrentCulture);
        }
        catch (FormatException)
        {
            return pattern;
        }
    }

    public static string Format(DateFormatDefinition format, DateTime moment)
    {
        try
        {
            return format.Layout switch
            {
                CalendarLayout.LunarMonthDay or CalendarLayout.LunarStemMonthDay or CalendarLayout.LunarFull
                    => ChineseLunarFormat.Format(format.Layout, moment),
                CalendarLayout.ThaiIso or CalendarLayout.ThaiSlash or CalendarLayout.ThaiEraYear
                    => ThaiBuddhistFormat.Format(format.Layout, moment),
                CalendarLayout.JapaneseDate or CalendarLayout.JapaneseDateTime or CalendarLayout.JapaneseShort
                    => JapaneseEraFormat.Format(format.Layout, moment),
                CalendarLayout.TaiwanNumeric or CalendarLayout.TaiwanFormal or CalendarLayout.TaiwanFull
                    => TaiwanMinguoFormat.Format(format.Layout, moment),
                _ => Format(format.Pattern, moment)
            };
        }
        catch (ArgumentOutOfRangeException)
        {
            return OutOfRangeText(format.Calendar);
        }
    }

    public static string FormatNow(string pattern) => Format(pattern, DateTime.Now);

    public static string ListText(DateFormatDefinition format)
    {
        string preview = Format(format, DateTime.Now);
        if (format.Calendar == CalendarSystem.Gregorian)
        {
            return preview + "    " + format.Pattern;
        }

        return Label(format.Calendar) + "    " + preview;
    }

    public static bool TryValidate(string pattern, out string preview)
    {
        preview = string.Empty;
        if (string.IsNullOrWhiteSpace(pattern))
        {
            return false;
        }

        try
        {
            preview = DateTime.Now.ToString(pattern, CultureInfo.CurrentCulture);
            return true;
        }
        catch (Exception ex) when (ex is FormatException or ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    public static DateFormatDefinition? Resolve(AppSettings settings, string id)
    {
        DateFormatDefinition? builtIn = BuiltIn.FirstOrDefault(item => item.Id == id);
        if (builtIn is not null)
        {
            return builtIn;
        }

        CustomFormat? custom = settings.CustomFormats.FirstOrDefault(item => item.Id == id);
        return custom is null
            ? null
            : new DateFormatDefinition(custom.Id, custom.Pattern, false, custom.Name);
    }

    public static string FormatResolved(AppSettings settings, string id, DateTime moment)
    {
        DateFormatDefinition format = Resolve(settings, id) ?? BuiltIn[0];
        return Format(format, moment);
    }

    public static IEnumerable<DateFormatDefinition> VisibleFormats(AppSettings settings)
    {
        HashSet<string> enabled = [.. settings.EnabledBuiltInIds];
        foreach (DateFormatDefinition item in BuiltIn)
        {
            if (enabled.Contains(item.Id))
            {
                yield return item;
            }
        }

        foreach (CustomFormat item in settings.CustomFormats)
        {
            yield return new DateFormatDefinition(item.Id, item.Pattern, false, item.Name);
        }
    }

    private static DateFormatDefinition Gregorian(string id, string pattern) => new(id, pattern, true);

    private static DateFormatDefinition Calendar(string id, CalendarSystem calendar, CalendarLayout layout) =>
        new(id, string.Empty, true, Calendar: calendar, Layout: layout);

    private static string Label(CalendarSystem calendar) => calendar switch
    {
        CalendarSystem.ChineseLunar => UiStrings.FormatLabelLunar,
        CalendarSystem.ThaiBuddhist => UiStrings.FormatLabelThai,
        CalendarSystem.Japanese => UiStrings.FormatLabelJapanese,
        CalendarSystem.Taiwan => UiStrings.FormatLabelTaiwan,
        _ => string.Empty
    };

    private static string OutOfRangeText(CalendarSystem calendar) => calendar switch
    {
        CalendarSystem.ChineseLunar => UiStrings.LunarOutOfRange,
        CalendarSystem.Japanese => UiStrings.JapaneseOutOfRange,
        CalendarSystem.Taiwan => UiStrings.TaiwanOutOfRange,
        _ => string.Empty
    };
}

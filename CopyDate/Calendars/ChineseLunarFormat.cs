using System.Globalization;

namespace CopyDate;

internal static class ChineseLunarFormat
{
    private static readonly ChineseLunisolarCalendar Calendar = new();
    private static readonly string[] Stems = ["", "甲", "乙", "丙", "丁", "戊", "己", "庚", "辛", "壬", "癸"];
    private static readonly string[] Branches = ["", "子", "丑", "寅", "卯", "辰", "巳", "午", "未", "申", "酉", "戌", "亥"];
    private static readonly string[] Months = ["", "正", "二", "三", "四", "五", "六", "七", "八", "九", "十", "冬", "腊"];
    private static readonly string[] MonthsTraditional = ["", "正", "二", "三", "四", "五", "六", "七", "八", "九", "十", "冬", "臘"];

    public static string Format(CalendarLayout layout, DateTime moment)
    {
        if (moment < Calendar.MinSupportedDateTime || moment > Calendar.MaxSupportedDateTime)
        {
            return UiStrings.LunarOutOfRange;
        }

        int year = Calendar.GetYear(moment);
        int slot = Calendar.GetMonth(moment);
        int leapSlot = Calendar.GetLeapMonth(year);
        bool isLeap = leapSlot != 0 && slot == leapSlot;
        int month = leapSlot != 0 && slot >= leapSlot ? slot - 1 : slot;
        int day = Calendar.GetDayOfMonth(moment);
        int sexagenary = Calendar.GetSexagenaryYear(moment);
        string stemBranch = Stems[Calendar.GetCelestialStem(sexagenary)] + Branches[Calendar.GetTerrestrialBranch(sexagenary)];
        string monthDay = MonthName(month, isLeap) + DayName(day);

        return layout switch
        {
            CalendarLayout.LunarStemMonthDay => stemBranch + "年" + monthDay,
            CalendarLayout.LunarFull => Prefix + stemBranch + "年" + monthDay,
            _ => Prefix + monthDay
        };
    }

    private static string MonthName(int month, bool isLeap)
    {
        string[] names = IsTraditional ? MonthsTraditional : Months;
        string leap = isLeap ? (IsTraditional ? "閏" : "闰") : string.Empty;
        return leap + names[month] + "月";
    }

    private static string DayName(int day)
    {
        string[] digits = ["", "一", "二", "三", "四", "五", "六", "七", "八", "九"];
        if (day <= 10)
        {
            return "初" + (day == 10 ? "十" : digits[day]);
        }

        if (day < 20)
        {
            return "十" + digits[day - 10];
        }

        if (day == 20)
        {
            return "二十";
        }

        if (day < 30)
        {
            return "廿" + digits[day - 20];
        }

        return "三十";
    }

    private static bool IsTraditional
    {
        get
        {
            for (CultureInfo? culture = CultureInfo.CurrentUICulture;
                 culture is not null && culture.Name.Length > 0;
                 culture = culture.Parent)
            {
                if (culture.Name.StartsWith("zh-Hant", StringComparison.OrdinalIgnoreCase)
                    || culture.Name is "zh-TW" or "zh-HK" or "zh-MO" or "zh-CHT")
                {
                    return true;
                }
            }

            return false;
        }
    }

    private static bool IsEnglish =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase);

    private static string Prefix => IsTraditional ? "農曆" : IsEnglish ? "Lunar " : "农历";
}

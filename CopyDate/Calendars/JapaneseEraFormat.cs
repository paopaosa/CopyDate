using System.Globalization;

namespace CopyDate;

internal static class JapaneseEraFormat
{
    private static readonly CultureInfo Culture = CreateCulture();

    public static string Format(CalendarLayout layout, DateTime moment)
    {
        Calendar calendar = Culture.DateTimeFormat.Calendar;
        if (moment < calendar.MinSupportedDateTime || moment > calendar.MaxSupportedDateTime)
        {
            return UiStrings.JapaneseOutOfRange;
        }

        if (layout == CalendarLayout.JapaneseShort)
        {
            return ShortDate(calendar, moment);
        }

        string date = moment.ToString("ggy年M月d日", Culture);
        if (layout == CalendarLayout.JapaneseDateTime)
        {
            return date + " " + moment.ToString("HH:mm", CultureInfo.InvariantCulture);
        }

        return date;
    }

    private static string ShortDate(Calendar calendar, DateTime moment)
    {
        int era = calendar.GetEra(moment);
        string eraName = Culture.DateTimeFormat.GetEraName(era);
        string letter = eraName switch
        {
            "明治" => "M",
            "大正" => "T",
            "昭和" => "S",
            "平成" => "H",
            "令和" => "R",
            _ => Culture.DateTimeFormat.GetAbbreviatedEraName(era)
        };

        int year = calendar.GetYear(moment);
        int month = calendar.GetMonth(moment);
        int day = calendar.GetDayOfMonth(moment);
        return $"{letter}{year}/{month:00}/{day:00}";
    }

    private static CultureInfo CreateCulture()
    {
        var culture = new CultureInfo("ja-JP", useUserOverride: false);
        culture.DateTimeFormat.Calendar = new JapaneseCalendar();
        return culture;
    }
}

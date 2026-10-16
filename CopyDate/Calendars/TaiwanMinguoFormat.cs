using System.Globalization;

namespace CopyDate;

internal static class TaiwanMinguoFormat
{
    private static readonly CultureInfo Culture = CreateCulture();

    public static string Format(CalendarLayout layout, DateTime moment)
    {
        Calendar calendar = Culture.DateTimeFormat.Calendar;
        if (moment < calendar.MinSupportedDateTime || moment > calendar.MaxSupportedDateTime)
        {
            return UiStrings.TaiwanOutOfRange;
        }

        int year = calendar.GetYear(moment);
        int month = calendar.GetMonth(moment);
        int day = calendar.GetDayOfMonth(moment);

        return layout switch
        {
            CalendarLayout.TaiwanFormal => $"民國{ChineseNumerals.Year(year)}年{ChineseNumerals.Number(month)}月{ChineseNumerals.Number(day)}日",
            CalendarLayout.TaiwanFull => $"中華民國{ChineseNumerals.Year(year)}年{ChineseNumerals.Number(month)}月{ChineseNumerals.Number(day)}日",
            _ => $"{year}/{month:00}/{day:00}"
        };
    }

    private static CultureInfo CreateCulture()
    {
        var culture = new CultureInfo("zh-TW", useUserOverride: false);
        culture.DateTimeFormat.Calendar = new TaiwanCalendar();
        return culture;
    }
}

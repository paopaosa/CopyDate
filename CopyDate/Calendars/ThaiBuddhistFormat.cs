using System.Globalization;

namespace CopyDate;

internal static class ThaiBuddhistFormat
{
    private static readonly CultureInfo Culture = CreateCulture();

    public static string Format(CalendarLayout layout, DateTime moment)
    {
        Calendar calendar = Culture.DateTimeFormat.Calendar;
        int year = calendar.GetYear(moment);
        int month = calendar.GetMonth(moment);
        int day = calendar.GetDayOfMonth(moment);

        return layout switch
        {
            CalendarLayout.ThaiSlash => $"{year:0000}/{month:00}/{day:00}",
            CalendarLayout.ThaiEraYear => $"พ.ศ. {year}",
            _ => $"{year:0000}-{month:00}-{day:00}"
        };
    }

    private static CultureInfo CreateCulture()
    {
        var culture = new CultureInfo("th-TH", useUserOverride: false);
        culture.DateTimeFormat.Calendar = new ThaiBuddhistCalendar();
        return culture;
    }
}

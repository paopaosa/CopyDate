using System.Globalization;

namespace CopyDate;

internal sealed record DateFormatDefinition(string Id, string Pattern, bool IsBuiltIn, string? Name = null);

internal static class DateFormatCatalog
{
    public const string IsoDate = "iso-date";
    public const string SlashDate = "slash-date";
    public const string ChineseDate = "cn-date";
    public const string CompactDate = "compact-date";
    public const string IsoDateTimeMinutes = "iso-datetime-min";
    public const string IsoDateTimeSeconds = "iso-datetime-sec";
    public const string ChineseDateTimeSeconds = "cn-datetime-sec";

    public static IReadOnlyList<DateFormatDefinition> BuiltIn { get; } =
    [
        new(IsoDate, "yyyy-MM-dd", true),
        new(SlashDate, "yyyy/MM/dd", true),
        new(ChineseDate, "yyyy年M月d日", true),
        new(CompactDate, "yyyyMMdd", true),
        new(IsoDateTimeMinutes, "yyyy-MM-dd HH:mm", true),
        new(IsoDateTimeSeconds, "yyyy-MM-dd HH:mm:ss", true),
        new(ChineseDateTimeSeconds, "yyyy年M月d日 HH:mm:ss", true)
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

    public static string FormatNow(string pattern) => Format(pattern, DateTime.Now);

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

    public static string? ResolvePattern(AppSettings settings, string id)
    {
        DateFormatDefinition? builtIn = BuiltIn.FirstOrDefault(item => item.Id == id);
        if (builtIn is not null)
        {
            return builtIn.Pattern;
        }

        return settings.CustomFormats.FirstOrDefault(item => item.Id == id)?.Pattern;
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
}

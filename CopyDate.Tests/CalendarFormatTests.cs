using System.Globalization;
using Xunit;

namespace CopyDate.Tests;

public class CalendarFormatTests
{
    private static DateFormatDefinition BuiltIn(string id) =>
        DateFormatCatalog.BuiltIn.First(item => item.Id == id);

    [Fact]
    public void Lunar_formats_use_month_names_stem_branch_and_leap_months()
    {
        using CultureScope culture = new("zh-Hans");
        DateTime today = new(2026, 9, 24);

        Assert.Equal("农历八月十四", Format(DateFormatCatalog.LunarDate, today));
        Assert.Equal("丙午年八月十四", Format(DateFormatCatalog.LunarStem, today));
        Assert.Equal("农历丙午年八月十四", Format(DateFormatCatalog.LunarFull, today));
        Assert.Equal("农历闰六月初一", Format(DateFormatCatalog.LunarDate, new DateTime(2025, 7, 25)));
        Assert.Equal("农历七月初一", Format(DateFormatCatalog.LunarDate, new DateTime(2025, 8, 23)));
        Assert.Equal("乙巳年腊月廿九", Format(DateFormatCatalog.LunarStem, new DateTime(2026, 2, 16)));
        Assert.Equal("超出农历支持范围（1901–2101）", Format(DateFormatCatalog.LunarDate, new DateTime(1901, 2, 18)));
    }

    [Fact]
    public void Lunar_text_follows_the_interface_language()
    {
        DateTime today = new(2026, 9, 24);

        using (new CultureScope("zh-Hant"))
        {
            Assert.Equal("農曆八月十四", Format(DateFormatCatalog.LunarDate, today));
            Assert.Equal("農曆閏六月初一", Format(DateFormatCatalog.LunarDate, new DateTime(2025, 7, 25)));
        }

        using (new CultureScope("en"))
        {
            Assert.Equal("Lunar 八月十四", Format(DateFormatCatalog.LunarDate, today));
            Assert.Equal("Lunar 丙午年八月十四", Format(DateFormatCatalog.LunarFull, today));
        }
    }

    [Fact]
    public void Thai_buddhist_formats_use_arabic_digits()
    {
        DateTime today = new(2026, 9, 24);

        Assert.Equal("2569-09-24", Format(DateFormatCatalog.BuddhistDate, today));
        Assert.Equal("2569/09/24", Format(DateFormatCatalog.BuddhistSlash, today));
        Assert.Equal("พ.ศ. 2569", Format(DateFormatCatalog.BuddhistEra, today));
    }

    [Fact]
    public void Japanese_era_formats_follow_era_boundaries()
    {
        using CultureScope culture = new("zh-Hans");
        DateTime today = new(2026, 9, 24, 10, 37, 0);

        Assert.Equal("令和8年9月24日", Format(DateFormatCatalog.JapaneseDate, today));
        Assert.Equal("令和8年9月24日 10:37", Format(DateFormatCatalog.JapaneseDateTime, today));
        Assert.Equal("R8/09/24", Format(DateFormatCatalog.JapaneseShort, today));
        Assert.Equal("令和元年5月1日", Format(DateFormatCatalog.JapaneseDate, new DateTime(2019, 5, 1)));
        Assert.Equal("R1/05/01", Format(DateFormatCatalog.JapaneseShort, new DateTime(2019, 5, 1)));
        Assert.Equal("平成31年4月30日", Format(DateFormatCatalog.JapaneseDate, new DateTime(2019, 4, 30)));
        Assert.Equal("H31/04/30", Format(DateFormatCatalog.JapaneseShort, new DateTime(2019, 4, 30)));
        Assert.Equal("昭和64年1月7日", Format(DateFormatCatalog.JapaneseDate, new DateTime(1989, 1, 7)));
        Assert.Equal("明治元年9月8日", Format(DateFormatCatalog.JapaneseDate, new DateTime(1868, 9, 8)));
        Assert.Equal("超出日本年号支持范围", Format(DateFormatCatalog.JapaneseDate, new DateTime(1868, 9, 7)));
    }

    [Fact]
    public void Taiwan_formal_dates_use_traditional_chinese_numerals()
    {
        using CultureScope culture = new("zh-Hans");

        Assert.Equal("115/09/24", Format(DateFormatCatalog.TaiwanDate, new DateTime(2026, 9, 24)));
        Assert.Equal("民國一百一十五年九月二十四日", Format(DateFormatCatalog.TaiwanFormal, new DateTime(2026, 9, 24)));
        Assert.Equal("中華民國一百一十五年九月二十四日", Format(DateFormatCatalog.TaiwanFull, new DateTime(2026, 9, 24)));
        Assert.Equal("民國十八年六月十八日", Format(DateFormatCatalog.TaiwanFormal, new DateTime(1929, 6, 18)));
        Assert.Equal("民國元年一月一日", Format(DateFormatCatalog.TaiwanFormal, new DateTime(1912, 1, 1)));
        Assert.Equal("1/01/01", Format(DateFormatCatalog.TaiwanDate, new DateTime(1912, 1, 1)));
        Assert.Equal("民國一百零一年一月一日", Format(DateFormatCatalog.TaiwanFormal, new DateTime(2012, 1, 1)));
        Assert.Equal("民國一百一十年十月三十一日", Format(DateFormatCatalog.TaiwanFormal, new DateTime(2021, 10, 31)));
        Assert.Equal("中華民國八千零八十八年一月一日", Format(DateFormatCatalog.TaiwanFull, new DateTime(9999, 1, 1)));
        Assert.Equal("超出民国纪年支持范围", Format(DateFormatCatalog.TaiwanDate, new DateTime(1911, 12, 31)));
    }

    [Fact]
    public void New_calendar_formats_stay_off_until_enabled()
    {
        var settings = new AppSettings();

        Assert.Equal(DateFormatCatalog.DefaultEnabledIds, settings.EnabledBuiltInIds);
        Assert.DoesNotContain(DateFormatCatalog.LunarDate, settings.EnabledBuiltInIds);
        Assert.DoesNotContain(DateFormatCatalog.JapaneseDate, DateFormatCatalog.VisibleFormats(settings).Select(item => item.Id));
    }

    private static string Format(string id, DateTime moment) =>
        DateFormatCatalog.Format(BuiltIn(id), moment);

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _culture;
        private readonly CultureInfo _uiCulture;

        public CultureScope(string name)
        {
            _culture = CultureInfo.CurrentCulture;
            _uiCulture = CultureInfo.CurrentUICulture;
            CultureInfo culture = CultureInfo.GetCultureInfo(name);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _culture;
            CultureInfo.CurrentUICulture = _uiCulture;
        }
    }
}

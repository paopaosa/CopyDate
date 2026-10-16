using System.Globalization;
using Xunit;

namespace CopyDate.Tests;

public class PatternReferenceTests
{
    [Theory]
    [InlineData("zh-Hans")]
    [InlineData("zh-Hant")]
    [InlineData("en")]
    public void Reference_codes_are_valid_format_patterns(string language)
    {
        CultureInfo culture = CultureInfo.CurrentCulture;
        CultureInfo uiCulture = CultureInfo.CurrentUICulture;
        CultureInfo? defaultCulture = CultureInfo.DefaultThreadCurrentCulture;
        CultureInfo? defaultUiCulture = CultureInfo.DefaultThreadCurrentUICulture;
        try
        {
            Localization.Apply(language);
            IReadOnlyList<PatternCode> codes = PatternReference.Parse(UiStrings.PatternReference);

            Assert.True(codes.Count >= 20);
            Assert.Contains(codes, item => item.Code == "yyyy-MM-dd");
            foreach (PatternCode item in codes)
            {
                Assert.True(DateFormatCatalog.TryValidate(item.Code, out string preview), item.Code);
                Assert.NotEqual(item.Code, preview);
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = uiCulture;
            CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = defaultUiCulture;
        }
    }
}

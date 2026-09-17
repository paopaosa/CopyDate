using System.Globalization;
using System.Resources;

namespace CopyDate;

internal static class Localization
{
    public const string FollowSystem = "system";
    public const string ChineseSimplified = "zh-Hans";
    public const string ChineseTraditional = "zh-Hant";
    public const string English = "en";

    public static void Apply(string language)
    {
        (CultureInfo culture, CultureInfo ui) = language switch
        {
            ChineseSimplified => (new CultureInfo("zh-CN"), new CultureInfo("zh-Hans")),
            ChineseTraditional => (new CultureInfo("zh-TW"), new CultureInfo("zh-Hant")),
            English => (new CultureInfo("en-US"), new CultureInfo("en")),
            _ => (CultureInfo.InstalledUICulture, CultureInfo.InstalledUICulture)
        };

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = ui;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = ui;
    }
}

internal static class UiStrings
{
    private static readonly ResourceManager Manager =
        new("CopyDate.Resources.Strings", typeof(UiStrings).Assembly);

    public static string AppName => Get(nameof(AppName));
    public static string TrayTooltip => Get(nameof(TrayTooltip));
    public static string CopiedTitle => Get(nameof(CopiedTitle));
    public static string CopiedText => Get(nameof(CopiedText));
    public static string CopyFailed => Get(nameof(CopyFailed));
    public static string MenuSettings => Get(nameof(MenuSettings));
    public static string MenuAbout => Get(nameof(MenuAbout));
    public static string MenuExit => Get(nameof(MenuExit));
    public static string SettingsTitle => Get(nameof(SettingsTitle));
    public static string Language => Get(nameof(Language));
    public static string LanguageSystem => Get(nameof(LanguageSystem));
    public static string LanguageZh => Get(nameof(LanguageZh));
    public static string LanguageZhHant => Get(nameof(LanguageZhHant));
    public static string LanguageEn => Get(nameof(LanguageEn));
    public static string TrayIconStyle => Get(nameof(TrayIconStyle));
    public static string TrayIconStyleRed => Get(nameof(TrayIconStyleRed));
    public static string TrayIconStyleGray => Get(nameof(TrayIconStyleGray));
    public static string TrayIconStyleBlue => Get(nameof(TrayIconStyleBlue));
    public static string DefaultFormat => Get(nameof(DefaultFormat));
    public static string BuiltInFormats => Get(nameof(BuiltInFormats));
    public static string CustomFormats => Get(nameof(CustomFormats));
    public static string Add => Get(nameof(Add));
    public static string Edit => Get(nameof(Edit));
    public static string Delete => Get(nameof(Delete));
    public static string Name => Get(nameof(Name));
    public static string Pattern => Get(nameof(Pattern));
    public static string Preview => Get(nameof(Preview));
    public static string StartWithWindows => Get(nameof(StartWithWindows));
    public static string ShowNotification => Get(nameof(ShowNotification));
    public static string Save => Get(nameof(Save));
    public static string Cancel => Get(nameof(Cancel));
    public static string CustomFormatTitle => Get(nameof(CustomFormatTitle));
    public static string InvalidPattern => Get(nameof(InvalidPattern));
    public static string NameRequired => Get(nameof(NameRequired));
    public static string PatternRequired => Get(nameof(PatternRequired));
    public static string PatternHelp => Get(nameof(PatternHelp));
    public static string AboutTitle => Get(nameof(AboutTitle));
    public static string AboutVersion => Get(nameof(AboutVersion));
    public static string AboutFree => Get(nameof(AboutFree));
    public static string AboutPrivacy => Get(nameof(AboutPrivacy));
    public static string AlreadyRunning => Get(nameof(AlreadyRunning));
    public static string StartupDisabledByUser => Get(nameof(StartupDisabledByUser));
    public static string StartupUnavailable => Get(nameof(StartupUnavailable));
    public static string Ok => Get(nameof(Ok));
    public static string ConfirmDelete => Get(nameof(ConfirmDelete));
    public static string SelectCustomFormat => Get(nameof(SelectCustomFormat));

    private static string Get(string name) =>
        Manager.GetString(name, CultureInfo.CurrentUICulture) ?? name;
}

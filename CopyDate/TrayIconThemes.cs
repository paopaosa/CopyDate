namespace CopyDate;

internal sealed record TrayIconTheme(string Id, Color Background, Color Border, Color Text);

internal static class TrayIconThemes
{
    public const string Red = "red";
    public const string Gray = "gray";
    public const string Blue = "blue";

    public static TrayIconTheme ClassicRed { get; } = new(
        Red,
        Color.White,
        Color.FromArgb(40, 40, 40),
        Color.FromArgb(220, 16, 16));

    public static TrayIconTheme SoftGray { get; } = new(
        Gray,
        Color.FromArgb(234, 234, 234),
        Color.FromArgb(168, 168, 168),
        Color.FromArgb(70, 70, 70));

    public static TrayIconTheme SteelBlue { get; } = new(
        Blue,
        Color.FromArgb(16, 58, 122),
        Color.FromArgb(12, 44, 96),
        Color.White);

    public static IReadOnlyList<TrayIconTheme> All { get; } = [ClassicRed, SoftGray, SteelBlue];

    public static TrayIconTheme Resolve(string? id) =>
        All.FirstOrDefault(item => item.Id == id) ?? ClassicRed;

    public static string DisplayName(string id) => id switch
    {
        Gray => UiStrings.TrayIconStyleGray,
        Blue => UiStrings.TrayIconStyleBlue,
        _ => UiStrings.TrayIconStyleRed
    };
}

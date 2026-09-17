namespace CopyDate;

internal sealed class AppSettings
{
    public string DefaultFormatId { get; set; } = DateFormatCatalog.IsoDate;
    public List<string> EnabledBuiltInIds { get; set; } = [.. DateFormatCatalog.BuiltIn.Select(item => item.Id)];
    public List<CustomFormat> CustomFormats { get; set; } = [];
    public string Language { get; set; } = Localization.FollowSystem;
    public string TrayIconStyle { get; set; } = TrayIconThemes.Red;
    public bool StartWithWindows { get; set; }
    public bool ShowCopyNotification { get; set; } = true;
}

internal sealed class CustomFormat
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
}

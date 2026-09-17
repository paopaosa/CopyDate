using Microsoft.Win32;
using Windows.ApplicationModel;

namespace CopyDate;

internal static class StartupService
{
    public const string StartupTaskId = "CopyDateStartup";
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string RunValueName = "CopyDate";

    public static async Task<bool> IsEnabledAsync()
    {
        if (PackageDetector.IsPackaged)
        {
            StartupTask task = await StartupTask.GetAsync(StartupTaskId);
            return task.State is StartupTaskState.Enabled or StartupTaskState.EnabledByPolicy;
        }

        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        return key?.GetValue(RunValueName) is string;
    }

    public static async Task<(bool Success, string? Message)> ApplyAsync(bool enable)
    {
        try
        {
            if (PackageDetector.IsPackaged)
            {
                return await ApplyPackagedAsync(enable);
            }

            ApplyUnpackaged(enable);
            return (true, null);
        }
        catch (Exception)
        {
            return (false, UiStrings.StartupUnavailable);
        }
    }

    private static async Task<(bool Success, string? Message)> ApplyPackagedAsync(bool enable)
    {
        StartupTask task = await StartupTask.GetAsync(StartupTaskId);
        if (enable)
        {
            switch (task.State)
            {
                case StartupTaskState.Enabled:
                case StartupTaskState.EnabledByPolicy:
                    return (true, null);
                case StartupTaskState.Disabled:
                    StartupTaskState state = await task.RequestEnableAsync();
                    if (state is StartupTaskState.Enabled or StartupTaskState.EnabledByPolicy)
                    {
                        return (true, null);
                    }

                    if (state == StartupTaskState.DisabledByUser)
                    {
                        return (false, UiStrings.StartupDisabledByUser);
                    }

                    return (false, UiStrings.StartupUnavailable);
                case StartupTaskState.DisabledByUser:
                    return (false, UiStrings.StartupDisabledByUser);
                default:
                    return (false, UiStrings.StartupUnavailable);
            }
        }

        if (task.State == StartupTaskState.Enabled)
        {
            task.Disable();
        }

        return (true, null);
    }

    private static void ApplyUnpackaged(bool enable)
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true)
            ?? Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);

        if (key is null)
        {
            throw new InvalidOperationException("Unable to open the Run registry key.");
        }

        if (enable)
        {
            key.SetValue(RunValueName, $"\"{Application.ExecutablePath}\"");
        }
        else if (key.GetValue(RunValueName) is not null)
        {
            key.DeleteValue(RunValueName, throwOnMissingValue: false);
        }
    }
}

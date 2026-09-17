namespace CopyDate;

internal static class Program
{
    internal const string MutexName = @"Local\CopyDate.SingleInstance";
    internal const string ActivateEventName = @"Local\CopyDate.Activate";

    [STAThread]
    private static void Main()
    {
        using var mutex = new Mutex(initiallyOwned: true, MutexName, out bool createdNew);
        using var activateEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ActivateEventName);

        if (!createdNew)
        {
            activateEvent.Set();
            return;
        }

        ApplicationConfiguration.Initialize();

        var settings = SettingsStore.Load();
        Localization.Apply(settings.Language);

        Application.Run(new TrayAppContext(settings, activateEvent));
    }
}

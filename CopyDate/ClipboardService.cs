using System.Runtime.InteropServices;

namespace CopyDate;

internal static class ClipboardService
{
    public static bool TryCopy(string text)
    {
        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                Clipboard.SetDataObject(text, copy: true, retryTimes: 5, retryDelay: 50);
                return true;
            }
            catch (ExternalException)
            {
                Thread.Sleep(50);
            }
            catch (ThreadStateException)
            {
                return false;
            }
        }

        return false;
    }
}

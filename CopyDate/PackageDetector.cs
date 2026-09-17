using System.Runtime.InteropServices;
using System.Text;

namespace CopyDate;

internal static class PackageDetector
{
    private const int AppModelErrorNoPackage = 15700;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int GetCurrentPackageFullName(ref int packageFullNameLength, StringBuilder? packageFullName);

    public static bool IsPackaged
    {
        get
        {
            int length = 0;
            int result = GetCurrentPackageFullName(ref length, null);
            return result != AppModelErrorNoPackage;
        }
    }
}

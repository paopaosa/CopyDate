using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Runtime.InteropServices;

namespace CopyDate;

internal static class AppIcon
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr handle);

    public static Icon Load(string? styleId = null) =>
        Create(DateTime.Today, Math.Max(32, SystemInformation.IconSize.Width), TrayIconThemes.Resolve(styleId));

    public static Icon CreateForTray(DateTime date, string? styleId = null)
    {
        int size = Math.Max(16, SystemInformation.SmallIconSize.Width);
        return Create(date, size, TrayIconThemes.Resolve(styleId));
    }

    public static Bitmap CreateBitmap(DateTime date, int size, TrayIconTheme theme)
    {
        var bitmap = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using Graphics g = Graphics.FromImage(bitmap);
        Draw(g, size, date, theme);
        return bitmap;
    }

    public static Icon Create(DateTime date, int size, TrayIconTheme theme)
    {
        using Bitmap bitmap = CreateBitmap(date, size, theme);
        IntPtr handle = bitmap.GetHicon();
        try
        {
            using var fromHandle = Icon.FromHandle(handle);
            return (Icon)fromHandle.Clone();
        }
        finally
        {
            DestroyIcon(handle);
        }
    }

    private static void Draw(Graphics g, int size, DateTime date, TrayIconTheme theme)
    {
        g.SmoothingMode = SmoothingMode.None;
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        g.TextRenderingHint = size <= 20
            ? TextRenderingHint.SingleBitPerPixelGridFit
            : TextRenderingHint.ClearTypeGridFit;
        g.Clear(theme.Background);

        int border = Math.Max(1, size / 16);
        using (var pen = new Pen(theme.Border, border))
        {
            int inset = border / 2;
            g.DrawRectangle(pen, inset, inset, size - border, size - border);
        }

        string text = date.Day.ToString(CultureInfo.InvariantCulture);
        var area = new Rectangle(border + 1, border, size - (border * 2) - 1, size - (border * 2));

        using Font font = FitFont(g, text, area);
        TextRenderer.DrawText(
            g,
            text,
            font,
            area,
            theme.Text,
            theme.Background,
            TextFormatFlags.HorizontalCenter
            | TextFormatFlags.VerticalCenter
            | TextFormatFlags.NoPadding
            | TextFormatFlags.SingleLine
            | TextFormatFlags.NoPrefix);
    }

    private static Font FitFont(Graphics g, string text, Rectangle area)
    {
        Font? chosen = null;
        for (float px = area.Height + 2; px >= 6f; px -= 0.5f)
        {
            var candidate = new Font("Segoe UI", px, FontStyle.Bold, GraphicsUnit.Pixel);
            Size measured = TextRenderer.MeasureText(
                g,
                text,
                candidate,
                area.Size,
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
            if (measured.Width <= area.Width && measured.Height <= area.Height)
            {
                chosen?.Dispose();
                chosen = candidate;
                break;
            }

            candidate.Dispose();
        }

        return chosen ?? new Font("Segoe UI", 8f, FontStyle.Bold, GraphicsUnit.Pixel);
    }
}

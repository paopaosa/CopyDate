using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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

    public static int TraySize => Math.Max(16, SystemInformation.SmallIconSize.Width);

    public static Icon CreateForTray(DateTime date, string? styleId = null) =>
        Create(date, TraySize, TrayIconThemes.Resolve(styleId));

    public static Bitmap CreateBitmap(DateTime date, int size, TrayIconTheme theme) =>
        CreateLabelBitmap(date.Day.ToString(CultureInfo.InvariantCulture), size, theme);

    public static Bitmap CreateLabelBitmap(string text, int size, TrayIconTheme theme)
    {
        var bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using Graphics g = Graphics.FromImage(bitmap);
        Draw(g, size, text, theme);
        return bitmap;
    }

    public static Bitmap CreateScrollFrame(Bitmap outgoing, Bitmap incoming, int offset)
    {
        int size = outgoing.Width;
        if (outgoing.Height != size || incoming.Width != size || incoming.Height != size)
        {
            throw new ArgumentException("Scroll frames must be square and the same size.");
        }

        offset = Math.Clamp(offset, 0, size);
        var frame = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        var bounds = new Rectangle(0, 0, size, size);
        BitmapData outgoingData = outgoing.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        BitmapData incomingData = incoming.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        BitmapData frameData = frame.LockBits(bounds, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        try
        {
            int outgoingRows = size - offset;
            if (outgoingRows > 0)
            {
                CopyRows(outgoingData, offset, frameData, 0, outgoingRows, size);
            }

            if (offset > 0)
            {
                CopyRows(incomingData, 0, frameData, outgoingRows, offset, size);
            }
        }
        finally
        {
            outgoing.UnlockBits(outgoingData);
            incoming.UnlockBits(incomingData);
            frame.UnlockBits(frameData);
        }

        return frame;
    }

    public static Icon ToIcon(Bitmap bitmap)
    {
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

    public static Icon Create(DateTime date, int size, TrayIconTheme theme)
    {
        using Bitmap bitmap = CreateBitmap(date, size, theme);
        return ToIcon(bitmap);
    }

    private static void CopyRows(BitmapData source, int sourceY, BitmapData destination, int destinationY, int rows, int width)
    {
        int bytesPerRow = width * 4;
        byte[] buffer = new byte[bytesPerRow];
        for (int row = 0; row < rows; row++)
        {
            Marshal.Copy(IntPtr.Add(source.Scan0, (sourceY + row) * source.Stride), buffer, 0, bytesPerRow);
            Marshal.Copy(buffer, 0, IntPtr.Add(destination.Scan0, (destinationY + row) * destination.Stride), bytesPerRow);
        }
    }

    private static void Draw(Graphics g, int size, string text, TrayIconTheme theme)
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

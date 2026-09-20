using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

internal static class Program
{
    private static int Main(string[] args)
    {
        string root = args.Length > 0
            ? args[0]
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

        string iconDir = Path.Combine(root, "CopyDate", "Assets");
        string packageImages = Path.Combine(root, "CopyDate.Package", "Images");
        Directory.CreateDirectory(iconDir);
        Directory.CreateDirectory(packageImages);

        int[] iconSizes = [16, 20, 24, 32, 48, 64, 128, 256];
        var pngs = new List<(int Size, byte[] Png)>();
        foreach (int size in iconSizes)
        {
            using Bitmap bitmap = DrawCalendar(size, transparent: true);
            pngs.Add((size, ToPng(bitmap)));
        }

        File.WriteAllBytes(Path.Combine(iconDir, "app.ico"), PackIcon(pngs));

        WritePng(DrawSquareLogo(44), Path.Combine(packageImages, "Square44x44Logo.png"));
        WritePng(DrawSquareLogo(150), Path.Combine(packageImages, "Square150x150Logo.png"));
        WritePng(DrawSquareLogo(310), Path.Combine(packageImages, "Square310x310Logo.png"));
        WritePng(DrawWideLogo(310, 150), Path.Combine(packageImages, "Wide310x150Logo.png"));
        WritePng(DrawSquareLogo(50), Path.Combine(packageImages, "StoreLogo.png"));
        WritePng(DrawWideLogo(620, 300), Path.Combine(packageImages, "SplashScreen.png"));

        string promoDir = Path.Combine(root, "store", "promo");
        Directory.CreateDirectory(promoDir);
        foreach (int size in new[] { 256, 512, 1024, 2048 })
        {
            WritePng(DrawCalendar(size, transparent: true), Path.Combine(promoDir, $"CopyDate-icon-{size}.png"));
            WritePng(DrawSquareLogo(size), Path.Combine(promoDir, $"CopyDate-logo-{size}.png"));
        }

        Console.WriteLine($"Wrote icons to {iconDir}");
        Console.WriteLine($"Wrote package images to {packageImages}");
        Console.WriteLine($"Wrote promo PNGs to {promoDir}");
        return 0;
    }

    private static Bitmap DrawCalendar(int size, bool transparent)
    {
        var bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using Graphics g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.Clear(transparent ? Color.Transparent : Color.FromArgb(43, 108, 176));

        float pad = Math.Max(1f, size * 0.08f);
        var rect = new RectangleF(pad, pad, size - pad * 2, size - pad * 2);
        float radius = Math.Max(2f, size * 0.16f);

        using (var bg = new SolidBrush(Color.FromArgb(37, 99, 168)))
        using (GraphicsPath path = RoundedRect(rect, radius))
        {
            g.FillPath(bg, path);
        }

        var header = new RectangleF(rect.X, rect.Y, rect.Width, Math.Max(3f, rect.Height * 0.28f));
        using (var headerBrush = new SolidBrush(Color.FromArgb(23, 64, 120)))
        using (GraphicsPath headerPath = RoundedRect(new RectangleF(header.X, header.Y, header.Width, header.Height + radius), radius))
        {
            g.SetClip(new RectangleF(rect.X, rect.Y, rect.Width, header.Height));
            g.FillPath(headerBrush, headerPath);
            g.ResetClip();
        }

        using (var ring = new SolidBrush(Color.White))
        {
            float ringW = Math.Max(1.2f, size * 0.07f);
            float ringH = Math.Max(1.6f, size * 0.12f);
            float top = rect.Y + Math.Max(1f, size * 0.04f);
            g.FillEllipse(ring, rect.X + rect.Width * 0.28f - ringW / 2, top, ringW, ringH);
            g.FillEllipse(ring, rect.X + rect.Width * 0.72f - ringW / 2, top, ringW, ringH);
        }

        if (size >= 24)
        {
            using var body = new SolidBrush(Color.FromArgb(245, 248, 252));
            var bodyRect = new RectangleF(
                rect.X + size * 0.08f,
                header.Bottom + size * 0.05f,
                rect.Width - size * 0.16f,
                rect.Bottom - header.Bottom - size * 0.1f);
            g.FillRectangle(body, bodyRect);

            using var textBrush = new SolidBrush(Color.FromArgb(23, 64, 120));
            float fontSize = Math.Max(6f, size * 0.36f);
            using var font = new Font("Segoe UI Semibold", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
            var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            g.DrawString("17", font, textBrush, bodyRect, format);
        }
        else
        {
            using var line = new Pen(Color.White, Math.Max(1f, size * 0.08f));
            float y = rect.Y + rect.Height * 0.62f;
            g.DrawLine(line, rect.X + size * 0.22f, y, rect.Right - size * 0.22f, y);
        }

        return bitmap;
    }

    private static Bitmap DrawSquareLogo(int size)
    {
        var bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using Graphics g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.FromArgb(43, 108, 176));
        int inner = (int)(size * 0.72);
        using Bitmap calendar = DrawCalendar(inner, transparent: true);
        g.DrawImage(calendar, (size - inner) / 2, (size - inner) / 2, inner, inner);
        return bitmap;
    }

    private static Bitmap DrawWideLogo(int width, int height)
    {
        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using Graphics g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.FromArgb(43, 108, 176));

        int iconSize = (int)(height * 0.62);
        using Bitmap calendar = DrawCalendar(iconSize, transparent: true);
        float iconX = height * 0.18f;
        float iconY = (height - iconSize) / 2f;
        g.DrawImage(calendar, iconX, iconY, iconSize, iconSize);

        using var brush = new SolidBrush(Color.White);
        float fontSize = Math.Max(18f, height * 0.22f);
        using var font = new Font("Segoe UI Semibold", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
        g.DrawString("CopyDate", font, brush, iconX + iconSize + height * 0.12f, (height - fontSize) / 2f - 4);
        return bitmap;
    }

    private static GraphicsPath RoundedRect(RectangleF rect, float radius)
    {
        float d = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    private static byte[] ToPng(Bitmap bitmap)
    {
        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        return stream.ToArray();
    }

    private static void WritePng(Bitmap bitmap, string path)
    {
        using (bitmap)
        {
            bitmap.Save(path, ImageFormat.Png);
        }
    }

    private static byte[] PackIcon(List<(int Size, byte[] Png)> images)
    {
        using var output = new MemoryStream();
        using var writer = new BinaryWriter(output);
        writer.Write((short)0);
        writer.Write((short)1);
        writer.Write((short)images.Count);

        int offset = 6 + (16 * images.Count);
        foreach ((int size, byte[] png) in images)
        {
            writer.Write((byte)(size >= 256 ? 0 : size));
            writer.Write((byte)(size >= 256 ? 0 : size));
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((short)1);
            writer.Write((short)32);
            writer.Write(png.Length);
            writer.Write(offset);
            offset += png.Length;
        }

        foreach ((_, byte[] png) in images)
        {
            writer.Write(png);
        }

        return output.ToArray();
    }
}

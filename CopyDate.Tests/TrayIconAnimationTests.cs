using System.Drawing.Imaging;
using Xunit;

namespace CopyDate.Tests;

public class TrayIconAnimationTests
{
    [Theory]
    [InlineData(16, 0, 0)]
    [InlineData(16, 190, 8)]
    [InlineData(16, 380, 16)]
    [InlineData(16, 500, 16)]
    [InlineData(24, -5, 0)]
    public void Offset_scrolls_the_full_icon_across_the_phase(int size, int elapsed, int expected)
    {
        Assert.Equal(expected, TrayIconMotion.Offset(size, elapsed));
    }

    [Fact]
    public void Phases_scroll_to_ok_hold_for_one_second_then_scroll_back()
    {
        Assert.False(TrayIconMotion.TryCompletePhase(TrayIconAnimPhase.ToOk, TrayIconMotion.ScrollMilliseconds - 1, out TrayIconAnimPhase stillScrolling));
        Assert.Equal(TrayIconAnimPhase.ToOk, stillScrolling);

        Assert.True(TrayIconMotion.TryCompletePhase(TrayIconAnimPhase.ToOk, TrayIconMotion.ScrollMilliseconds, out TrayIconAnimPhase holding));
        Assert.Equal(TrayIconAnimPhase.Hold, holding);

        Assert.False(TrayIconMotion.TryCompletePhase(TrayIconAnimPhase.Hold, TrayIconMotion.HoldMilliseconds - 1, out TrayIconAnimPhase stillHolding));
        Assert.Equal(TrayIconAnimPhase.Hold, stillHolding);

        Assert.True(TrayIconMotion.TryCompletePhase(TrayIconAnimPhase.Hold, TrayIconMotion.HoldMilliseconds, out TrayIconAnimPhase returning));
        Assert.Equal(TrayIconAnimPhase.ToDate, returning);

        Assert.True(TrayIconMotion.TryCompletePhase(TrayIconAnimPhase.ToDate, TrayIconMotion.ScrollMilliseconds, out TrayIconAnimPhase done));
        Assert.Equal(TrayIconAnimPhase.Done, done);
        Assert.Equal(1000, TrayIconMotion.HoldMilliseconds);
    }

    [Fact]
    public void Scroll_frame_moves_the_whole_icon_up_and_brings_the_next_one_from_below()
    {
        const int size = 4;
        using Bitmap outgoing = SolidRows(size, row => Color.FromArgb(255, 20 * (row + 1), 0, 0));
        using Bitmap incoming = SolidRows(size, row => Color.FromArgb(255, 0, 0, 30 * (row + 1)));

        using Bitmap start = AppIcon.CreateScrollFrame(outgoing, incoming, 0);
        Assert.Equal(outgoing.GetPixel(1, 0), start.GetPixel(1, 0));
        Assert.Equal(outgoing.GetPixel(1, 3), start.GetPixel(1, 3));

        using Bitmap middle = AppIcon.CreateScrollFrame(outgoing, incoming, 2);
        Assert.Equal(outgoing.GetPixel(1, 2), middle.GetPixel(1, 0));
        Assert.Equal(outgoing.GetPixel(1, 3), middle.GetPixel(1, 1));
        Assert.Equal(incoming.GetPixel(1, 0), middle.GetPixel(1, 2));
        Assert.Equal(incoming.GetPixel(1, 1), middle.GetPixel(1, 3));

        using Bitmap end = AppIcon.CreateScrollFrame(outgoing, incoming, size);
        Assert.Equal(incoming.GetPixel(1, 0), end.GetPixel(1, 0));
        Assert.Equal(incoming.GetPixel(1, 3), end.GetPixel(1, 3));
    }

    [Fact]
    public void Ok_label_replaces_the_day_number_on_the_same_icon()
    {
        TrayIconTheme theme = TrayIconThemes.ClassicRed;
        using Bitmap day = AppIcon.CreateBitmap(new DateTime(2026, 10, 15), 32, theme);
        using Bitmap ok = AppIcon.CreateLabelBitmap(TrayIconMotion.CopiedLabel, 32, theme);

        Assert.False(SamePixels(day, ok));
    }

    private static Bitmap SolidRows(int size, Func<int, Color> colorForRow)
    {
        var bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        for (int y = 0; y < size; y++)
        {
            Color color = colorForRow(y);
            for (int x = 0; x < size; x++)
            {
                bitmap.SetPixel(x, y, color);
            }
        }

        return bitmap;
    }

    private static bool SamePixels(Bitmap left, Bitmap right)
    {
        if (left.Width != right.Width || left.Height != right.Height)
        {
            return false;
        }

        for (int y = 0; y < left.Height; y++)
        {
            for (int x = 0; x < left.Width; x++)
            {
                if (left.GetPixel(x, y).ToArgb() != right.GetPixel(x, y).ToArgb())
                {
                    return false;
                }
            }
        }

        return true;
    }
}

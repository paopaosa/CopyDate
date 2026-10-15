namespace CopyDate;

internal enum TrayIconAnimPhase
{
    ToOk,
    Hold,
    ToDate,
    Done
}

internal static class TrayIconMotion
{
    public const int ScrollMilliseconds = 380;
    public const int HoldMilliseconds = 1000;
    public const int FrameIntervalMilliseconds = 32;
    public const string CopiedLabel = "OK";

    public static int Offset(int size, int elapsedMilliseconds, int durationMilliseconds = ScrollMilliseconds)
    {
        if (size <= 0 || elapsedMilliseconds <= 0)
        {
            return 0;
        }

        if (durationMilliseconds <= 0 || elapsedMilliseconds >= durationMilliseconds)
        {
            return Math.Max(0, size);
        }

        return (int)Math.Round(
            size * (elapsedMilliseconds / (double)durationMilliseconds),
            MidpointRounding.AwayFromZero);
    }

    public static bool TryCompletePhase(TrayIconAnimPhase phase, int elapsedMilliseconds, out TrayIconAnimPhase next)
    {
        int limit = phase switch
        {
            TrayIconAnimPhase.ToOk => ScrollMilliseconds,
            TrayIconAnimPhase.Hold => HoldMilliseconds,
            TrayIconAnimPhase.ToDate => ScrollMilliseconds,
            _ => 0
        };

        if (phase == TrayIconAnimPhase.Done || elapsedMilliseconds < limit)
        {
            next = phase;
            return false;
        }

        next = phase switch
        {
            TrayIconAnimPhase.ToOk => TrayIconAnimPhase.Hold,
            TrayIconAnimPhase.Hold => TrayIconAnimPhase.ToDate,
            _ => TrayIconAnimPhase.Done
        };
        return true;
    }
}

internal sealed class TrayIconAnimator : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly Action _completed;
    private readonly System.Windows.Forms.Timer _timer;
    private Bitmap? _dateBitmap;
    private Bitmap? _okBitmap;
    private Icon? _frameIcon;
    private TrayIconAnimPhase _phase;
    private long _phaseStart;
    private bool _running;

    public TrayIconAnimator(NotifyIcon notifyIcon, Action completed)
    {
        _notifyIcon = notifyIcon;
        _completed = completed;
        _timer = new System.Windows.Forms.Timer { Interval = TrayIconMotion.FrameIntervalMilliseconds };
        _timer.Tick += (_, _) => Advance();
    }

    public bool IsRunning => _running;

    public void Play(DateTime date, string? styleId)
    {
        if (_running)
        {
            return;
        }

        ReleaseBitmaps();
        TrayIconTheme theme = TrayIconThemes.Resolve(styleId);
        int size = AppIcon.TraySize;
        _dateBitmap = AppIcon.CreateBitmap(date, size, theme);
        _okBitmap = AppIcon.CreateLabelBitmap(TrayIconMotion.CopiedLabel, size, theme);
        _phase = TrayIconAnimPhase.ToOk;
        _phaseStart = Environment.TickCount64;
        _running = true;
        _timer.Start();
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Dispose();
        _running = false;
        ReleaseBitmaps();
        ReleaseFrame();
    }

    private void Advance()
    {
        if (!_running || _dateBitmap is null || _okBitmap is null)
        {
            return;
        }

        int elapsed = (int)(Environment.TickCount64 - _phaseStart);
        if (TrayIconMotion.TryCompletePhase(_phase, elapsed, out TrayIconAnimPhase next))
        {
            _phase = next;
            _phaseStart = Environment.TickCount64;
            if (next == TrayIconAnimPhase.Hold)
            {
                ShowBitmap(_okBitmap);
                return;
            }

            if (next == TrayIconAnimPhase.Done)
            {
                Finish();
                return;
            }
        }

        int offset = TrayIconMotion.Offset(_dateBitmap.Width, (int)(Environment.TickCount64 - _phaseStart));
        if (_phase == TrayIconAnimPhase.ToOk)
        {
            ShowFrame(AppIcon.CreateScrollFrame(_dateBitmap, _okBitmap, offset));
            return;
        }

        if (_phase == TrayIconAnimPhase.ToDate)
        {
            ShowFrame(AppIcon.CreateScrollFrame(_okBitmap, _dateBitmap, offset));
        }
    }

    private void Finish()
    {
        _timer.Stop();
        _running = false;
        ReleaseBitmaps();
        _completed();
        ReleaseFrame();
    }

    private void ShowBitmap(Bitmap bitmap)
    {
        SwapFrame(AppIcon.ToIcon(bitmap));
    }

    private void ShowFrame(Bitmap frame)
    {
        using (frame)
        {
            SwapFrame(AppIcon.ToIcon(frame));
        }
    }

    private void SwapFrame(Icon icon)
    {
        Icon? previous = _frameIcon;
        _frameIcon = icon;
        _notifyIcon.Icon = icon;
        previous?.Dispose();
    }

    private void ReleaseBitmaps()
    {
        _dateBitmap?.Dispose();
        _okBitmap?.Dispose();
        _dateBitmap = null;
        _okBitmap = null;
    }

    private void ReleaseFrame()
    {
        Icon? frame = _frameIcon;
        _frameIcon = null;
        frame?.Dispose();
    }
}

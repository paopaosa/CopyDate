using System.ComponentModel;

namespace CopyDate;

internal sealed class TrayAppContext : ApplicationContext
{
    private readonly RegisteredWaitHandle _activateWait;
    private readonly SynchronizationContext _ui;
    private readonly NotifyIcon _notifyIcon;
    private readonly TrayIconAnimator _copyAnimation;
    private readonly ContextMenuStrip _menu;
    private readonly System.Windows.Forms.Timer _leftClickTimer;
    private Icon _icon;
    private readonly System.Windows.Forms.Timer _dateTimer;
    private DateTime _iconDate;
    private AppSettings _settings;
    private bool _settingsOpen;
    private bool _aboutOpen;
    private bool _refreshIconAfterAnimation;
    private bool _pendingLeftClickCopy;

    public TrayAppContext(AppSettings settings, EventWaitHandle activateEvent)
    {
        _settings = settings;
        _ui = SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext();
        _iconDate = DateTime.Today;
        _icon = AppIcon.CreateForTray(_iconDate, _settings.TrayIconStyle);
        _menu = new ContextMenuStrip();
        _menu.Opening += MenuOnOpening;

        _notifyIcon = new NotifyIcon
        {
            Icon = _icon,
            Visible = true,
            ContextMenuStrip = _menu,
            Text = UiStrings.AppName
        };
        _notifyIcon.MouseClick += NotifyIconOnMouseClick;
        _notifyIcon.MouseDoubleClick += NotifyIconOnMouseDoubleClick;
        _copyAnimation = new TrayIconAnimator(_notifyIcon, RestoreIconAfterCopyAnimation);

        _leftClickTimer = new System.Windows.Forms.Timer
        {
            Interval = Math.Max(50, SystemInformation.DoubleClickTime)
        };
        _leftClickTimer.Tick += LeftClickTimerOnTick;

        UpdateTooltip();

        _dateTimer = new System.Windows.Forms.Timer { Interval = 60_000 };
        _dateTimer.Tick += (_, _) => RefreshTrayIconIfDateChanged();
        _dateTimer.Start();

        _activateWait = ThreadPool.RegisterWaitForSingleObject(
            activateEvent,
            OnSecondInstance,
            null,
            Timeout.Infinite,
            executeOnlyOnce: false);
    }

    private void OnSecondInstance(object? state, bool timedOut)
    {
        _ui.Post(_ => ShowAlreadyRunningBalloon(), null);
    }

    private void ShowAlreadyRunningBalloon()
    {
        if (!_notifyIcon.Visible)
        {
            return;
        }

        _notifyIcon.BalloonTipTitle = UiStrings.AppName;
        _notifyIcon.BalloonTipText = UiStrings.AlreadyRunning;
        _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
        _notifyIcon.ShowBalloonTip(2000);
    }

    private void NotifyIconOnMouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        _pendingLeftClickCopy = true;
        _leftClickTimer.Stop();
        _leftClickTimer.Start();
    }

    private void NotifyIconOnMouseDoubleClick(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        CancelPendingLeftClickCopy();
        CopyDefault(playSound: true);
    }

    private void LeftClickTimerOnTick(object? sender, EventArgs e)
    {
        _leftClickTimer.Stop();
        if (!_pendingLeftClickCopy)
        {
            return;
        }

        _pendingLeftClickCopy = false;
        CopyDefault(playSound: false);
    }

    private void CancelPendingLeftClickCopy()
    {
        _pendingLeftClickCopy = false;
        _leftClickTimer.Stop();
    }

    private void MenuOnOpening(object? sender, CancelEventArgs e)
    {
        RebuildMenu();
    }

    private void RebuildMenu()
    {
        _menu.Items.Clear();
        DateTime now = DateTime.Now;

        foreach (DateFormatDefinition format in DateFormatCatalog.VisibleFormats(_settings))
        {
            string preview = DateFormatCatalog.Format(format, now);
            string formatId = format.Id;
            var item = new ToolStripMenuItem(preview);
            item.Click += (_, _) => CopyText(DateFormatCatalog.FormatResolved(_settings, formatId, DateTime.Now), playSound: false);
            _menu.Items.Add(item);
        }

        if (_menu.Items.Count > 0)
        {
            _menu.Items.Add(new ToolStripSeparator());
        }

        _menu.Items.Add(UiStrings.MenuSettings, null, (_, _) => OpenSettings());
        _menu.Items.Add(UiStrings.MenuAbout, null, (_, _) => OpenAbout());
        _menu.Items.Add(new ToolStripSeparator());
        _menu.Items.Add(UiStrings.MenuExit, null, (_, _) => ExitThread());
    }

    private void CopyDefault(bool playSound)
    {
        CopyText(DateFormatCatalog.FormatResolved(_settings, _settings.DefaultFormatId, DateTime.Now), playSound);
    }

    private void CopyText(string text, bool playSound)
    {
        if (!ClipboardService.TryCopy(text))
        {
            _notifyIcon.BalloonTipTitle = UiStrings.CopiedTitle;
            _notifyIcon.BalloonTipText = UiStrings.CopyFailed;
            _notifyIcon.BalloonTipIcon = ToolTipIcon.Error;
            _notifyIcon.ShowBalloonTip(2500);
            return;
        }

        _copyAnimation.Play(_iconDate, _settings.TrayIconStyle);

        if (playSound && _settings.PlayCopySoundOnDoubleClick)
        {
            CopyFeedbackSound.Play();
        }

        if (!_settings.ShowCopyNotification)
        {
            return;
        }

        _notifyIcon.BalloonTipTitle = UiStrings.CopiedTitle;
        _notifyIcon.BalloonTipText = string.Format(UiStrings.CopiedText, text);
        _notifyIcon.BalloonTipIcon = ToolTipIcon.None;
        _notifyIcon.ShowBalloonTip(1500);
    }

    private void OpenSettings()
    {
        if (_settingsOpen)
        {
            return;
        }

        _settingsOpen = true;
        try
        {
            AppSettings working = SettingsStore.Clone(_settings);
            using var form = new SettingsForm(working);
            if (form.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            SettingsStore.Save(working);
            _settings = working;
            Localization.Apply(_settings.Language);
            ApplyTrayIcon();
            UpdateTooltip();
        }
        finally
        {
            _settingsOpen = false;
        }
    }

    private void OpenAbout()
    {
        if (_aboutOpen)
        {
            return;
        }

        _aboutOpen = true;
        try
        {
            using var form = new AboutForm();
            form.ShowDialog();
        }
        finally
        {
            _aboutOpen = false;
        }
    }

    private void RefreshTrayIconIfDateChanged()
    {
        if (DateTime.Today == _iconDate)
        {
            return;
        }

        ApplyTrayIcon();
    }

    private void ApplyTrayIcon()
    {
        if (_copyAnimation.IsRunning)
        {
            _refreshIconAfterAnimation = true;
            return;
        }

        Icon previous = _icon;
        _iconDate = DateTime.Today;
        _icon = AppIcon.CreateForTray(_iconDate, _settings.TrayIconStyle);
        _notifyIcon.Icon = _icon;
        previous.Dispose();
    }

    private void RestoreIconAfterCopyAnimation()
    {
        if (_refreshIconAfterAnimation || DateTime.Today != _iconDate)
        {
            _refreshIconAfterAnimation = false;
            ApplyTrayIcon();
            return;
        }

        _notifyIcon.Icon = _icon;
    }

    private void UpdateTooltip()
    {
        _notifyIcon.Text = UiStrings.TrayTooltip;
    }

    protected override void ExitThreadCore()
    {
        CancelPendingLeftClickCopy();
        _leftClickTimer.Dispose();
        _dateTimer.Stop();
        _dateTimer.Dispose();
        _activateWait.Unregister(null);
        _notifyIcon.Icon = _icon;
        _copyAnimation.Dispose();
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _menu.Dispose();
        _icon.Dispose();
        base.ExitThreadCore();
    }
}

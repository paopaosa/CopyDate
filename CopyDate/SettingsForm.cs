namespace CopyDate;

internal sealed class SettingsForm : Form
{
    private readonly AppSettings _settings;
    private readonly ComboBox _languageBox;
    private readonly ComboBox _defaultFormatBox;
    private readonly CheckedListBox _builtInList;
    private readonly ListView _customList;
    private readonly CheckBox _startupBox;
    private readonly CheckBox _notificationBox;
    private readonly Button _saveButton;
    private readonly List<StylePreview> _stylePreviews = [];

    public SettingsForm(AppSettings settings)
    {
        _settings = settings;
        Text = UiStrings.SettingsTitle;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = false;
        ShowInTaskbar = true;
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = SystemFonts.MessageBoxFont;
        MinimumSize = new Size(700, 700);
        ClientSize = PreferredClientSize(1120, 1120);
        Icon = AppIcon.Load(settings.TrayIconStyle);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            ColumnCount = 2,
            RowCount = 9
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _languageBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Dock = DockStyle.Fill
        };
        _languageBox.Items.Add(new LanguageOption(Localization.FollowSystem, UiStrings.LanguageSystem));
        _languageBox.Items.Add(new LanguageOption(Localization.ChineseSimplified, UiStrings.LanguageZh));
        _languageBox.Items.Add(new LanguageOption(Localization.ChineseTraditional, UiStrings.LanguageZhHant));
        _languageBox.Items.Add(new LanguageOption(Localization.English, UiStrings.LanguageEn));
        _languageBox.SelectedItem = _languageBox.Items.Cast<LanguageOption>()
            .FirstOrDefault(item => item.Id == settings.Language) ?? _languageBox.Items[0];

        _defaultFormatBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Dock = DockStyle.Fill
        };

        _builtInList = new CheckedListBox
        {
            Dock = DockStyle.Fill,
            CheckOnClick = true,
            IntegralHeight = false,
            HorizontalScrollbar = true
        };

        var customHeader = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 4,
            RowCount = 1
        };
        customHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        customHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        customHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        customHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        customHeader.Controls.Add(new Label
        {
            Text = UiStrings.CustomFormats,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 10, 0, 6)
        }, 0, 0);

        var addButton = new Button { Text = UiStrings.Add, AutoSize = true, Margin = new Padding(4, 6, 0, 6) };
        var editButton = new Button { Text = UiStrings.Edit, AutoSize = true, Margin = new Padding(4, 6, 0, 6) };
        var deleteButton = new Button { Text = UiStrings.Delete, AutoSize = true, Margin = new Padding(4, 6, 0, 6) };
        addButton.Click += (_, _) => AddCustom();
        editButton.Click += (_, _) => EditCustom();
        deleteButton.Click += (_, _) => DeleteCustom();
        customHeader.Controls.Add(addButton, 1, 0);
        customHeader.Controls.Add(editButton, 2, 0);
        customHeader.Controls.Add(deleteButton, 3, 0);

        _customList = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            HideSelection = false,
            MultiSelect = false
        };
        _customList.Columns.Add(UiStrings.Name, 280);
        _customList.Columns.Add(UiStrings.Pattern, 320);
        _customList.Columns.Add(UiStrings.Preview, 360);
        _customList.DoubleClick += (_, _) => EditCustom();

        _startupBox = new CheckBox
        {
            Text = UiStrings.StartWithWindows,
            AutoSize = true,
            Margin = new Padding(0, 10, 0, 4)
        };
        _notificationBox = new CheckBox
        {
            Text = UiStrings.ShowNotification,
            AutoSize = true,
            Checked = settings.ShowCopyNotification,
            Margin = new Padding(0, 4, 0, 8)
        };

        var buttons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            WrapContents = false,
            AutoSize = true
        };
        _saveButton = new Button { Text = UiStrings.Save, AutoSize = true };
        var cancel = new Button { Text = UiStrings.Cancel, DialogResult = DialogResult.Cancel, AutoSize = true };
        _saveButton.Click += async (_, _) => await SaveAsync();
        buttons.Controls.Add(_saveButton);
        buttons.Controls.Add(cancel);

        root.Controls.Add(new Label { Text = UiStrings.Language, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 8, 12, 8) }, 0, 0);
        root.Controls.Add(_languageBox, 1, 0);
        root.Controls.Add(new Label { Text = UiStrings.DefaultFormat, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 8, 12, 8) }, 0, 1);
        root.Controls.Add(_defaultFormatBox, 1, 1);
        root.Controls.Add(new Label { Text = UiStrings.TrayIconStyle, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 12, 12, 8) }, 0, 2);
        root.Controls.Add(CreateStylePicker(), 1, 2);
        var builtInLabel = new Label { Text = UiStrings.BuiltInFormats, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 12, 0, 4) };
        root.Controls.Add(builtInLabel, 0, 3);
        root.SetColumnSpan(builtInLabel, 2);
        root.Controls.Add(_builtInList, 0, 4);
        root.SetColumnSpan(_builtInList, 2);
        root.Controls.Add(customHeader, 0, 5);
        root.SetColumnSpan(customHeader, 2);
        root.Controls.Add(_customList, 0, 6);
        root.SetColumnSpan(_customList, 2);

        var checks = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true
        };
        checks.Controls.Add(_startupBox);
        checks.Controls.Add(_notificationBox);
        root.Controls.Add(checks, 0, 7);
        root.SetColumnSpan(checks, 2);
        root.Controls.Add(buttons, 0, 8);
        root.SetColumnSpan(buttons, 2);

        Controls.Add(root);
        AcceptButton = _saveButton;
        CancelButton = cancel;

        LoadBuiltIn();
        LoadCustom();
        RefreshDefaultFormatList();
        Shown += async (_, _) => await LoadStartupAsync();
    }

    private Control CreateStylePicker()
    {
        var flow = new FlowLayoutPanel
        {
            AutoSize = true,
            WrapContents = true,
            Margin = new Padding(0, 4, 0, 8)
        };

        foreach (TrayIconTheme theme in TrayIconThemes.All)
        {
            var card = new Panel
            {
                Width = 168,
                Height = 96,
                Margin = new Padding(0, 0, 12, 4),
                Padding = new Padding(8),
                Cursor = Cursors.Hand,
                Tag = theme.Id
            };
            var picture = new PictureBox
            {
                Size = new Size(48, 48),
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Image = AppIcon.CreateBitmap(DateTime.Today, 48, theme)
            };
            var label = new Label
            {
                AutoSize = false,
                Location = new Point(66, 12),
                Size = new Size(90, 72),
                Text = TrayIconThemes.DisplayName(theme.Id)
            };
            card.Controls.Add(picture);
            card.Controls.Add(label);
            card.Click += (_, _) => SelectStyle(theme.Id);
            picture.Click += (_, _) => SelectStyle(theme.Id);
            label.Click += (_, _) => SelectStyle(theme.Id);
            _stylePreviews.Add(new StylePreview(theme.Id, card));
            flow.Controls.Add(card);
        }

        SelectStyle(_settings.TrayIconStyle);
        return flow;
    }

    private void SelectStyle(string id)
    {
        _settings.TrayIconStyle = TrayIconThemes.Resolve(id).Id;
        foreach (StylePreview preview in _stylePreviews)
        {
            bool selected = preview.Id == _settings.TrayIconStyle;
            preview.Card.BackColor = selected ? Color.FromArgb(232, 240, 252) : SystemColors.Window;
            preview.Card.BorderStyle = BorderStyle.FixedSingle;
        }
    }

    private static Size PreferredClientSize(int width, int height)
    {
        Screen screen = Screen.FromPoint(Cursor.Position);
        Rectangle working = screen.WorkingArea;
        const int margin = 48;
        return new Size(
            Math.Min(width, Math.Max(700, working.Width - margin)),
            Math.Min(height, Math.Max(700, working.Height - margin)));
    }

    private async Task LoadStartupAsync()
    {
        try
        {
            _startupBox.Checked = await StartupService.IsEnabledAsync() || _settings.StartWithWindows;
        }
        catch
        {
            _startupBox.Checked = _settings.StartWithWindows;
        }
    }

    private void LoadBuiltIn()
    {
        HashSet<string> enabled = [.. _settings.EnabledBuiltInIds];
        _builtInList.Items.Clear();
        foreach (DateFormatDefinition item in DateFormatCatalog.BuiltIn)
        {
            var entry = new FormatEntry(item.Id, item.Pattern, DateFormatCatalog.FormatNow(item.Pattern) + "    " + item.Pattern);
            _builtInList.Items.Add(entry, enabled.Contains(item.Id));
        }
    }

    private void LoadCustom()
    {
        _customList.Items.Clear();
        foreach (CustomFormat format in _settings.CustomFormats)
        {
            var item = new ListViewItem(format.Name) { Tag = format };
            item.SubItems.Add(format.Pattern);
            item.SubItems.Add(DateFormatCatalog.FormatNow(format.Pattern));
            _customList.Items.Add(item);
        }
    }

    private void RefreshDefaultFormatList()
    {
        string selectedId = _settings.DefaultFormatId;
        _defaultFormatBox.Items.Clear();

        foreach (DateFormatDefinition item in DateFormatCatalog.BuiltIn)
        {
            _defaultFormatBox.Items.Add(new FormatEntry(item.Id, item.Pattern, DateFormatCatalog.FormatNow(item.Pattern)));
        }

        foreach (CustomFormat format in _settings.CustomFormats)
        {
            _defaultFormatBox.Items.Add(new FormatEntry(format.Id, format.Pattern, $"{format.Name}: {DateFormatCatalog.FormatNow(format.Pattern)}"));
        }

        object? match = _defaultFormatBox.Items.Cast<FormatEntry>().FirstOrDefault(item => item.Id == selectedId);
        _defaultFormatBox.SelectedItem = match ?? _defaultFormatBox.Items[0];
    }

    private void AddCustom()
    {
        using var dialog = new CustomFormatDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _settings.CustomFormats.Add(new CustomFormat
        {
            Name = dialog.FormatName,
            Pattern = dialog.Pattern
        });
        LoadCustom();
        RefreshDefaultFormatList();
    }

    private void EditCustom()
    {
        if (_customList.SelectedItems.Count == 0 || _customList.SelectedItems[0].Tag is not CustomFormat format)
        {
            MessageBox.Show(this, UiStrings.SelectCustomFormat, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new CustomFormatDialog(format.Name, format.Pattern);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        format.Name = dialog.FormatName;
        format.Pattern = dialog.Pattern;
        LoadCustom();
        RefreshDefaultFormatList();
    }

    private void DeleteCustom()
    {
        if (_customList.SelectedItems.Count == 0 || _customList.SelectedItems[0].Tag is not CustomFormat format)
        {
            MessageBox.Show(this, UiStrings.SelectCustomFormat, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (MessageBox.Show(this, UiStrings.ConfirmDelete, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        _settings.CustomFormats.Remove(format);
        if (_settings.DefaultFormatId == format.Id)
        {
            _settings.DefaultFormatId = DateFormatCatalog.IsoDate;
        }

        LoadCustom();
        RefreshDefaultFormatList();
    }

    private async Task SaveAsync()
    {
        _saveButton.Enabled = false;
        try
        {
            _settings.Language = ((LanguageOption)_languageBox.SelectedItem!).Id;
            _settings.DefaultFormatId = ((FormatEntry)_defaultFormatBox.SelectedItem!).Id;
            _settings.EnabledBuiltInIds = _builtInList.CheckedItems.Cast<FormatEntry>().Select(item => item.Id).ToList();
            _settings.ShowCopyNotification = _notificationBox.Checked;

            (bool success, string? message) = await StartupService.ApplyAsync(_startupBox.Checked);
            if (!success)
            {
                MessageBox.Show(this, message ?? UiStrings.StartupUnavailable, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                _startupBox.Checked = false;
            }

            _settings.StartWithWindows = _startupBox.Checked;
            DialogResult = DialogResult.OK;
        }
        finally
        {
            _saveButton.Enabled = true;
        }
    }

    private sealed record LanguageOption(string Id, string Label)
    {
        public override string ToString() => Label;
    }

    private sealed record FormatEntry(string Id, string Pattern, string Display)
    {
        public override string ToString() => Display;
    }

    private sealed record StylePreview(string Id, Panel Card);
}

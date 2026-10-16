namespace CopyDate;

internal sealed class CustomFormatDialog : Form
{
    private readonly TextBox _nameBox;
    private readonly TextBox _patternBox;
    private readonly Label _previewValue;

    public string FormatName => _nameBox.Text.Trim();
    public string Pattern => _patternBox.Text.Trim();

    public CustomFormatDialog(string? name = null, string? pattern = null)
    {
        Text = UiStrings.CustomFormatTitle;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = SystemFonts.MessageBoxFont;
        ClientSize = new Size(440, 230);
        Icon = AppIcon.Load();

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 2,
            RowCount = 5
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _nameBox = new TextBox { Dock = DockStyle.Fill, Text = name ?? string.Empty };
        _patternBox = new TextBox { Dock = DockStyle.Fill, Text = pattern ?? string.Empty };
        _previewValue = new Label { AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 8, 0, 8) };

        layout.Controls.Add(new Label { Text = UiStrings.Name, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 6, 12, 6) }, 0, 0);
        layout.Controls.Add(_nameBox, 1, 0);
        layout.Controls.Add(new Label { Text = UiStrings.Pattern, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 6, 12, 6) }, 0, 1);
        layout.Controls.Add(_patternBox, 1, 1);
        layout.Controls.Add(new Label { Text = UiStrings.Preview, AutoSize = true, Anchor = AnchorStyles.Left }, 0, 2);
        layout.Controls.Add(_previewValue, 1, 2);
        var helpLabel = new Label
        {
            Text = UiStrings.PatternHelp,
            AutoSize = true,
            ForeColor = SystemColors.GrayText,
            Margin = new Padding(0, 4, 0, 8)
        };
        layout.Controls.Add(helpLabel, 0, 3);
        layout.SetColumnSpan(helpLabel, 2);

        var buttons = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 1
        };
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        var help = new Button { Text = UiStrings.PatternHelpButton, AutoSize = true, Anchor = AnchorStyles.Left };
        help.Click += (_, _) => PatternHelpDialog.ShowHelp(this);
        var actions = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            WrapContents = false,
            Anchor = AnchorStyles.Right
        };
        var ok = new Button { Text = UiStrings.Ok, AutoSize = true };
        var cancel = new Button { Text = UiStrings.Cancel, DialogResult = DialogResult.Cancel, AutoSize = true };
        ok.Click += (_, _) => TryAccept();
        actions.Controls.Add(ok);
        actions.Controls.Add(cancel);
        buttons.Controls.Add(help, 0, 0);
        buttons.Controls.Add(actions, 1, 0);
        layout.Controls.Add(buttons, 0, 4);
        layout.SetColumnSpan(buttons, 2);

        AcceptButton = ok;
        CancelButton = cancel;
        Controls.Add(layout);

        _patternBox.TextChanged += (_, _) => UpdatePreview();
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (DateFormatCatalog.TryValidate(_patternBox.Text, out string preview))
        {
            _previewValue.Text = preview;
            _previewValue.ForeColor = SystemColors.ControlText;
        }
        else
        {
            _previewValue.Text = UiStrings.InvalidPattern;
            _previewValue.ForeColor = Color.Firebrick;
        }
    }

    private void TryAccept()
    {
        if (string.IsNullOrWhiteSpace(FormatName))
        {
            MessageBox.Show(this, UiStrings.NameRequired, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            _nameBox.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(Pattern))
        {
            MessageBox.Show(this, UiStrings.PatternRequired, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            _patternBox.Focus();
            return;
        }

        if (!DateFormatCatalog.TryValidate(Pattern, out _))
        {
            MessageBox.Show(this, UiStrings.InvalidPattern, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _patternBox.Focus();
            return;
        }

        DialogResult = DialogResult.OK;
    }
}

namespace CopyDate;

internal sealed class AboutForm : Form
{
    public AboutForm()
    {
        Text = UiStrings.AboutTitle;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = SystemFonts.MessageBoxFont;
        ClientSize = new Size(420, 240);
        Icon = AppIcon.Load();

        string version = AssemblyVersion();
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 1,
            RowCount = 5
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(new Label
        {
            Text = UiStrings.AppName,
            AutoSize = true,
            Font = new Font(Font.FontFamily, 16, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 8)
        });
        layout.Controls.Add(new Label
        {
            Text = string.Format(UiStrings.AboutVersion, version),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 12)
        });
        layout.Controls.Add(new Label
        {
            Text = UiStrings.AboutFree + Environment.NewLine + Environment.NewLine + UiStrings.AboutPrivacy,
            Dock = DockStyle.Fill,
            AutoSize = false
        });

        var ok = new Button
        {
            Text = UiStrings.Ok,
            DialogResult = DialogResult.OK,
            Anchor = AnchorStyles.Right,
            AutoSize = true
        };
        AcceptButton = ok;
        CancelButton = ok;
        layout.Controls.Add(ok);

        Controls.Add(layout);
    }

    private static string AssemblyVersion()
    {
        return typeof(AboutForm).Assembly.GetName().Version?.ToString(3) ?? "1.0.0";
    }
}

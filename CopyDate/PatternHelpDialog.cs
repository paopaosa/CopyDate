using System.Globalization;

namespace CopyDate;

internal readonly record struct PatternCode(string Code, string Description);

internal static class PatternReference
{
    public static IReadOnlyList<PatternCode> Parse(string text)
    {
        List<PatternCode> items = [];
        foreach (string raw in text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            string line = raw.Trim();
            int split = line.IndexOf('|');
            if (split <= 0 || split == line.Length - 1)
            {
                continue;
            }

            items.Add(new PatternCode(line[..split].Trim(), line[(split + 1)..].Trim()));
        }

        return items;
    }
}

internal sealed class PatternHelpDialog : Form
{
    public PatternHelpDialog()
    {
        Text = UiStrings.PatternHelpTitle;
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = SystemFonts.MessageBoxFont;
        MinimumSize = new Size(640, 480);
        ClientSize = new Size(760, 560);
        Icon = AppIcon.Load();

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 1,
            RowCount = 3
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var list = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            HeaderStyle = ColumnHeaderStyle.Nonclickable,
            MultiSelect = false
        };
        list.Columns.Add(UiStrings.FormatCode, 210);
        list.Columns.Add(UiStrings.FormatMeaning, 320);
        list.Columns.Add(UiStrings.FormatExample, 180);
        foreach (PatternCode item in PatternReference.Parse(UiStrings.PatternReference))
        {
            string example = DateFormatCatalog.TryValidate(item.Code, out string preview) ? preview : item.Code;
            list.Items.Add(new ListViewItem([item.Code, item.Description, example]));
        }

        var note = new Label
        {
            Text = UiStrings.PatternHelpNote,
            AutoSize = true,
            Margin = new Padding(0, 12, 0, 12)
        };
        void FitNote() => note.MaximumSize = new Size(Math.Max(200, ClientSize.Width - 32), 0);
        FitNote();
        Resize += (_, _) => FitNote();
        var close = new Button { Text = UiStrings.Ok, AutoSize = true, DialogResult = DialogResult.OK };
        var buttons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false
        };
        buttons.Controls.Add(close);
        AcceptButton = close;
        CancelButton = close;

        layout.Controls.Add(list, 0, 0);
        layout.Controls.Add(note, 0, 1);
        layout.Controls.Add(buttons, 0, 2);
        Controls.Add(layout);
    }

    public static void ShowHelp(IWin32Window owner)
    {
        using var dialog = new PatternHelpDialog();
        dialog.ShowDialog(owner);
    }
}
